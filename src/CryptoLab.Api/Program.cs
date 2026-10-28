using System.Text.Json;
using CryptoLab.Api;
using CryptoLab.Common;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IRsaService, RsaService>();
builder.Services.AddSingleton<IAesService, AesService>();
builder.Services.AddSingleton<KeyStorage>();
builder.Services.AddSingleton<SessionStorage>();

var app = builder.Build();

// При старті генеруємо 10 пар RSA ключів
var rsaService = app.Services.GetRequiredService<IRsaService>();
var keyStorage = app.Services.GetRequiredService<KeyStorage>();
for (var i = 0; i < 10; i++)
    keyStorage.Add(rsaService.GenerateCryptoKeys());

app.UseSwagger();
app.UseSwaggerUI();

// ---------- Завдання 1 ----------
app.MapPost("/api/crypto-keys/generate/rsa-keys", (IRsaService rsa, KeyStorage storage) =>
{
    try
    {
        var id = storage.Add(rsa.GenerateCryptoKeys());
        return Results.Ok(id);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/crypto-keys/rsa-public-key/{id:int}", (int id, KeyStorage storage) =>
{
    var keys = storage.Get(id);
    return keys is null ? Results.NotFound() : Results.Text(keys.PublicKey);
});

// ---------- Завдання 3 ----------
// a) клієнт отримує випадковий публічний ключ + заголовок x-rsa-id
app.MapGet("/api/secure/public-key", (KeyStorage storage, HttpContext ctx) =>
{
    var (id, keys) = storage.GetRandom();
    ctx.Response.Headers["x-rsa-id"] = id.ToString();
    return Results.Text(keys.PublicKey);
});

// e) сервер розшифровує дані сесії та зберігає
app.MapPost("/api/secure/session", (
    [FromHeader(Name = "x-rsa-id")] int rsaId,
    EncryptedRequest request,
    KeyStorage keys,
    SessionStorage sessions,
    IRsaService rsa) =>
{
    var pair = keys.Get(rsaId);
    if (pair is null) return Results.NotFound(new { error = "RSA key pair not found" });

    try
    {
        var json = rsa.Decrypt(pair.PrivateKey, request.Data);
        var payload = JsonSerializer.Deserialize<SessionInitPayload>(json);
        if (payload is null || string.IsNullOrEmpty(payload.SessionId))
            return Results.BadRequest(new { error = "Invalid session payload" });

        sessions.Set(payload.SessionId, new AesKey { Key = payload.Key, IV = payload.IV });
        return Results.Ok(new { message = "Session key saved" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = "Cannot decrypt data: " + ex.Message });
    }
});

// g) сервер дешифрує повідомлення, додає час, шифрує і повертає
app.MapPost("/api/secure/message", (
    [FromHeader(Name = "x-session-id")] string sessionId,
    EncryptedRequest request,
    SessionStorage sessions,
    IAesService aes) =>
{
    var key = sessions.Get(sessionId);
    if (key is null) return Results.NotFound(new { error = "Session not found" });

    try
    {
        var plain = aes.Decrypt(key, request.Data);
        var reply = $"{plain}. Received: {DateTime.UtcNow:O}";
        return Results.Ok(new EncryptedRequest(aes.Encrypt(key, reply)));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = "Cannot decrypt message: " + ex.Message });
    }
});

app.Run();