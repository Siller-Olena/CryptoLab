using System.Net.Http.Json;
using System.Text.Json;
using CryptoLab.Common;

// адреса сервера: перший аргумент без "--", інакше 8080; "--tamper" імітує зміну даних
var tamper = args.Contains("--tamper");
var baseUrl = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "http://localhost:8080";
using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };
var rsa = new RsaService();
var aes = new AesService();
var hasher = new HashService();

// поточна сесія зберігається між повідомленнями
string? sessionId = null;
AesKey? aesKey = null;

// показує код відповіді та текст помилки сервера
async Task EnsureOk(HttpResponseMessage response)
{
    if (response.IsSuccessStatusCode) return;
    var body = await response.Content.ReadAsStringAsync();
    throw new Exception($"{(int)response.StatusCode} {response.ReasonPhrase}: {body}");
}

// a-d) створення нової сесії: публічний ключ, AES-ключ, IV, номер сесії, відправка серверу
async Task CreateSessionAsync()
{
    var keyResponse = await http.GetAsync("/api/secure/public-key");
    await EnsureOk(keyResponse);
    var rsaId = keyResponse.Headers.GetValues("x-rsa-id").First();
    var publicKey = await keyResponse.Content.ReadAsStringAsync();

    aesKey = aes.GenerateSecretKey();
    sessionId = Guid.NewGuid().ToString();

    var payload = new SessionInitPayload { SessionId = sessionId, Key = aesKey.Key, IV = aesKey.IV };
    var encryptedPayload = rsa.Encrypt(publicKey, JsonSerializer.Serialize(payload));

    var sessionRequest = new HttpRequestMessage(HttpMethod.Post, "/api/secure/session")
    {
        Content = JsonContent.Create(new EncryptedRequest(encryptedPayload))
    };
    sessionRequest.Headers.Add("x-rsa-id", rsaId);
    await EnsureOk(await http.SendAsync(sessionRequest));
    Console.WriteLine($"Створено нову сесію: {sessionId}");
}

while (true)
{
    Console.Write("\nВведіть повідомлення (порожній рядок — вихід): ");
    var message = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(message)) break;

    try
    {
        var sent = false;
        for (var attempt = 0; attempt < 2 && !sent; attempt++)
        {
            if (sessionId is null || aesKey is null)
                await CreateSessionAsync();

            // хеш від справжніх даних; у режимі --tamper дані "змінюються" після розрахунку хеша
            var secure = new SecureMessage(tamper ? message + "!" : message, hasher.ComputeHash(message));
            var json = JsonSerializer.Serialize(secure);

            var msgRequest = new HttpRequestMessage(HttpMethod.Post, "/api/secure/message")
            {
                Content = JsonContent.Create(new EncryptedRequest(aes.Encrypt(aesKey!, json)))
            };
            msgRequest.Headers.Add("x-session-id", sessionId);
            var msgResponse = await http.SendAsync(msgRequest);

            // сервер каже, що сесія застаріла: скидаємо її і повторюємо з новою
            if ((int)msgResponse.StatusCode == 440)
            {
                Console.WriteLine("Сервер повернув 440: сесія застаріла. Створюю нову сесію...");
                sessionId = null;
                aesKey = null;
                continue;
            }

            await EnsureOk(msgResponse);

            var encryptedReply = await msgResponse.Content.ReadFromJsonAsync<EncryptedRequest>();
            var reply = aes.Decrypt(aesKey!, encryptedReply!.Data);

            Console.WriteLine($"Відповідь сервера: {reply}");

            // сервер повертає час у UTC, користувачу показуємо локальний час
            var marker = "Received: ";
            var idx = reply.LastIndexOf(marker);
            if (idx >= 0 && DateTimeOffset.TryParse(reply[(idx + marker.Length)..], out var utcTime))
                Console.WriteLine($"Час отримання (локальний): {utcTime.ToLocalTime():dd.MM.yyyy HH:mm:ss}");

            Console.WriteLine($"Номер сесії: {sessionId}");
            sent = true;
        }
if (!sent) Console.WriteLine("Не вдалося відправити повідомлення.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Помилка: " + ex.Message);
    }
}