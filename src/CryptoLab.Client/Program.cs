using System.Net.Http.Json;
using System.Text.Json;
using CryptoLab.Common;

var baseUrl = args.Length > 0 ? args[0] : "http://localhost:8080";
using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };
var rsa = new RsaService();
var aes = new AesService();

while (true)
{
    Console.Write("\nВведіть повідомлення (порожній рядок — вихід): ");
    var message = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(message)) break;

    try
    {
        // a) отримуємо публічний ключ і x-rsa-id
        var keyResponse = await http.GetAsync("/api/secure/public-key");
        keyResponse.EnsureSuccessStatusCode();
        var rsaId = keyResponse.Headers.GetValues("x-rsa-id").First();
        var publicKey = await keyResponse.Content.ReadAsStringAsync();

        // b, c) AES-ключ, IV та номер сесії
        var aesKey = aes.GenerateSecretKey();
        var sessionId = Guid.NewGuid().ToString();

        // d) шифруємо публічним ключем і відправляємо
        var payload = new SessionInitPayload { SessionId = sessionId, Key = aesKey.Key, IV = aesKey.IV };
        var encryptedPayload = rsa.Encrypt(publicKey, JsonSerializer.Serialize(payload));

        var sessionRequest = new HttpRequestMessage(HttpMethod.Post, "/api/secure/session")
        {
            Content = JsonContent.Create(new EncryptedRequest(encryptedPayload))
        };
        sessionRequest.Headers.Add("x-rsa-id", rsaId);
        (await http.SendAsync(sessionRequest)).EnsureSuccessStatusCode();
        Console.WriteLine("Сервер підтвердив отримання ключа сесії.");

        // f) шифруємо повідомлення AES і відправляємо
        var msgRequest = new HttpRequestMessage(HttpMethod.Post, "/api/secure/message")
        {
            Content = JsonContent.Create(new EncryptedRequest(aes.Encrypt(aesKey, message)))
        };
        msgRequest.Headers.Add("x-session-id", sessionId);
        var msgResponse = await http.SendAsync(msgRequest);
        msgResponse.EnsureSuccessStatusCode();

        // h) дешифруємо відповідь і друкуємо
        var encryptedReply = await msgResponse.Content.ReadFromJsonAsync<EncryptedRequest>();
        var reply = aes.Decrypt(aesKey, encryptedReply!.Data);

        Console.WriteLine($"Відповідь сервера: {reply}");
        Console.WriteLine($"Номер сесії: {sessionId}");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Помилка: " + ex.Message);
    }
}