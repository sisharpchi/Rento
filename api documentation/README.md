# Rento API — To'liq hujjat

Rento API barcha endpointlari va ularning request/response formatlari.

**Base URL:** `https://<host>/` (masalan: `https://localhost:5xxx/` yoki AppHost orqali `apiservice`)

**Swagger UI:** `GET /swagger` — brauzerda interaktiv hujjat  
**OpenAPI JSON:** `GET /swagger/v1/swagger.json`

---

## Mundarija

1. [Health](#1-health)
2. [OAuth 2.0 / OpenID Connect (Token)](#2-oauth-20--openid-connect-token)
3. [Telegram Auth (Mini App & Bot)](#3-telegram-auth-mini-app--bot)

---

## 1. Health

Development muhitida qo'llaniladi. Production da endpointlar ochiq bo'lmasligi mumkin.

| Method | Endpoint   | Tavsif |
|--------|------------|--------|
| GET    | `/health`  | Barcha health check'lar — ilova trafik qabul qilishga tayyor yoki yo'q |
| GET    | `/alive`   | Faqat "live" teglangan check'lar — ilova tirik yoki yo'q |

**Javob:** 200 OK (Healthy) yoki 503 (Unhealthy).

---

## 2. OAuth 2.0 / OpenID Connect (Token)

**Prefix:** `security/oauth`  
**Content-Type:** `application/x-www-form-urlencoded` (form body)

### 2.1 Token olish — `POST /security/oauth/token`

Access token (va ixtiyoriy refresh token) olish uchun. Qo'llab-quvvatlanadigan grant turlari:

- **client_credentials** — client ID + client secret (ma’lumotlar bazasidagi OpenIddict application)
- **password** — username (yoki email) + password — foydalanuvchi uchun access + refresh token
- **refresh_token** — refresh token orqali yangi access (va yangi refresh) token

#### Request (form body)

| Grant type           | Form fieldlar (majburiy) |
|----------------------|---------------------------|
| client_credentials   | `grant_type=client_credentials`, `client_id`, `client_secret`, ixtiyoriy `scope` |
| password             | `grant_type=password`, `username`, `password`, ixtiyoriy `scope` (offline_access — refresh token uchun) |
| refresh_token        | `grant_type=refresh_token`, `refresh_token` |

#### Muvaffaqiyatli javob (200 OK)

```json
{
  "token_type": "Bearer",
  "access_token": "...",
  "expires_in": 86400,
  "refresh_token": "..."   // faqat password yoki refresh_token grant da, offline_access scope bilan
}
```

- Access token muddati: 24 soat (sozlamadan).
- Refresh token muddati: 7 kun (sozlamadan).

#### Xato javoblari

| HTTP | error                  | error_description |
|------|------------------------|-------------------|
| 400  | unsupported_grant_type | Grant type qo'llab-quvvatlanmaydi |
| 400  | invalid_request        | So'rov noto'g'ri (masalan username/password bo'sh) |
| 401  | invalid_grant          | Foydalanuvchi topilmadi yoki parol noto'g'ri |

---

### 2.2 Tokenni bekor qilish — `POST /security/oauth/revoke`

Refresh token yoki boshqa tokenni bekor qilish (RFC 7009).

#### Request (form body)

| Field | Tavsif |
|-------|--------|
| token | Bekor qilinadigan token (odatda refresh_token) |

#### Javob

- **200 OK** — token bekor qilindi yoki topilmadi / allaqachon bekor qilingan (RFC 7009 bo'yicha har doim 200).
- **400 Bad Request** — `token` berilmagan: `error: invalid_request`, `error_description: "The token to revoke is required."`

---

## 3. Telegram Auth (Mini App & Bot)

**Prefix:** `api/auth/telegram`  
**Content-Type:** `application/json` (body), response ham `application/json`.

Bot endpointlari **X-Bot-Secret** header talab qiladi (sozlama: `TelegramBot:SecretKey`). Mini App endpointlari bu headerni talab qilmaydi.

---

### 3.1 Mini App — Ro'yxatdan o'tish / ulash — `POST /api/auth/telegram/register`

Mini App start: telefon raqam va Telegram user id orqali foydalanuvchini ro'yxatdan o'tkazish yoki mavjud foydalanuvchini Telegram bilan ulash.

**Auth:** kerak emas.

#### Request body

```json
{
  "phoneNumber": "+998901234567",
  "telegramUserId": 123456789
}
```

| Maydon          | Tur   | Majburiy | Tavsif            |
|-----------------|-------|----------|-------------------|
| phoneNumber     | string| Ha       | Telefon raqam     |
| telegramUserId  | long  | Ha       | Telegram user id  |

#### Javob

**200 OK** — muvaffaqiyat:

```json
{
  "success": true
}
```

**400 Bad Request** — xato (masalan noto'g'ri telefon):

```json
{
  "success": false,
  "error": "Xato xabari",
  "errorCode": 40001
}
```

**ErrorCode:** `40001` = InvalidPhone, `40000` = InvalidRequest.

---

### 3.2 Mini App — Login uchun kod yaratish — `POST /api/auth/telegram/generate-code`

Faqat telefon raqam orqali login kodi yaratiladi. Kod 2 daqiqa amal qiladi. Foydalanuvchi allaqachon bot/Mini App orqali ulangan bo'lishi kerak.

**Auth:** kerak emas.

#### Request body

```json
{
  "phoneNumber": "+998901234567"
}
```

| Maydon      | Tur   | Majburiy | Tavsif   |
|-------------|-------|----------|----------|
| phoneNumber | string| Ha       | Telefon  |

#### Javob

**200 OK** — muvaffaqiyat:

```json
{
  "success": true
}
```

**404 Not Found** — foydalanuvchi topilmadi (`errorCode: 40401`):

```json
{
  "success": false,
  "error": "User not found",
  "errorCode": 40401
}
```

**400 Bad Request** — boshqa xatolar (masalan kod muddati tugagan: `40002` CodeExpired).

---

### 3.3 Bot — Telegram user uchun kod olish — `POST /api/auth/telegram/code-for-bot`

Bot: Telegram user id bo'yicha login kodini olish. **X-Bot-Secret** header majburiy.

#### Headers

| Header        | Tavsif                    |
|---------------|---------------------------|
| X-Bot-Secret  | Sozlama: `TelegramBot:SecretKey` |
| Content-Type  | application/json          |

#### Request body

```json
{
  "telegramUserId": 123456789
}
```

| Maydon         | Tur  | Majburiy | Tavsif           |
|----------------|------|----------|------------------|
| telegramUserId | long | Ha       | Telegram user id |

#### Javob

**200 OK** — muvaffaqiyat (to'g'ridan-to'g'ri `TelegramBotCodeResponse`):

```json
{
  "code": "1234",
  "expiresAtUtc": "2025-03-02T12:34:00Z",
  "regenerated": false
}
```

| Maydon        | Tavsif                                      |
|---------------|---------------------------------------------|
| code          | 4 xonali kod                                |
| expiresAtUtc  | Kodning amal qilish muddati (UTC)           |
| regenerated   | true — yangi kod generatsiya qilindi; false — eski kod qaytarildi |

**401 Unauthorized** — `X-Bot-Secret` yo'q yoki noto'g'ri.

**404 Not Found** — bu Telegram user uchun kod yo'q (`errorCode: 40402` NoCodeForTelegramUser):

```json
{
  "success": false,
  "error": "...",
  "errorCode": 40402
}
```

---

### 3.4 Bot — Tilni o'rnatish — `POST /api/auth/telegram/set-language`

Bot: foydalanuvchi tilini o'rnatish (uz, ru, en). **X-Bot-Secret** majburiy.

#### Headers

| Header        | Tavsif                    |
|---------------|---------------------------|
| X-Bot-Secret  | Sozlama: `TelegramBot:SecretKey` |

#### Request body

```json
{
  "telegramUserId": 123456789,
  "language": "uz"
}
```

| Maydon         | Tur   | Majburiy | Tavsif           |
|----------------|-------|----------|------------------|
| telegramUserId | long  | Ha       | Telegram user id |
| language       | string| Ha       | uz, ru, en       |

#### Javob

**200 OK** — muvaffaqiyat:

```json
{
  "success": true
}
```

**401 Unauthorized** — X-Bot-Secret noto'g'ri.  
**404 Not Found** — foydalanuvchi topilmadi (`errorCode: 40401`).  
**400 Bad Request** — boshqa xato.

---

### 3.5 Bot — Foydalanuvchini yaratish/yangilash (Ensure User) — `POST /api/auth/telegram/ensure-user`

Bot `/start`: Telegram ma’lumotlari (TelegramUserId, FirstName, LastName, UserName, ixtiyoriy PhoneNumber) orqali foydalanuvchini yaratadi yoki mavjudini yangilaydi. **X-Bot-Secret** majburiy.

#### Headers

| Header        | Tavsif                    |
|---------------|---------------------------|
| X-Bot-Secret  | Sozlama: `TelegramBot:SecretKey` |

#### Request body

```json
{
  "telegramUserId": 123456789,
  "firstName": "Ism",
  "lastName": "Familiya",
  "userName": "username",
  "phoneNumber": "+998901234567"
}
```

| Maydon         | Tur   | Majburiy | Tavsif              |
|----------------|-------|----------|---------------------|
| telegramUserId | long  | Ha       | Telegram user id    |
| firstName      | string| Yo'q     | Ism                 |
| lastName       | string| Yo'q     | Familiya            |
| userName       | string| Yo'q     | Telegram username   |
| phoneNumber    | string| Yo'q     | Telefon (keyinroq)   |

#### Javob

**200 OK** — muvaffaqiyat:

```json
{
  "success": true
}
```

**401 Unauthorized** — X-Bot-Secret noto'g'ri.  
**400 Bad Request** — so'rov xatosi.

---

### 3.6 Bot — Profil olish — `GET /api/auth/telegram/profile`

Bot: Telegram user id bo'yicha profil. **X-Bot-Secret** majburiy.

#### Headers

| Header        | Tavsif                    |
|---------------|---------------------------|
| X-Bot-Secret  | Sozlama: `TelegramBot:SecretKey` |

#### Query parametrlar

| Parametr       | Tur  | Majburiy | Tavsif           |
|----------------|------|----------|------------------|
| telegramUserId | long | Ha       | Telegram user id |

**Misol:** `GET /api/auth/telegram/profile?telegramUserId=123456789`

#### Javob

**200 OK** — muvaffaqiyat:

```json
{
  "firstName": "Ism",
  "lastName": "Familiya",
  "phoneNumber": "+998901234567",
  "telegramId": 123456789,
  "language": "uz"
}
```

| Maydon       | Tur   | Tavsif          |
|--------------|-------|-----------------|
| firstName    | string| Ism             |
| lastName     | string| Familiya        |
| phoneNumber  | string| Telefon         |
| telegramId   | long  | Telegram user id|
| language     | string| uz / ru / en    |

**401 Unauthorized** — X-Bot-Secret noto'g'ri.  
**404 Not Found** — foydalanuvchi topilmadi (`errorCode: 40401`).

---

## Umumiy xato kodlari (ErrorCode)

| Kod   | Nomi                 | Tavsif                          |
|-------|----------------------|----------------------------------|
| 40000 | InvalidRequest       | Noto'g'ri so'rov                |
| 40001 | InvalidPhone         | Noto'g'ri telefon               |
| 40002 | CodeExpired          | Kod muddati tugagan             |
| 40401 | UserNotFound         | Foydalanuvchi topilmadi         |
| 40402 | NoCodeForTelegramUser| Bu Telegram user uchun kod yo'q |

---

## ResponseResult tuzilishi

Telegram va boshqa API’larda ishlatiladigan umumiy javob:

- **Muvaffaqiyat (Success):**  
  `{ "success": true }` yoki `{ "success": true, "value": { ... } }`
- **Xato:**  
  `{ "success": false, "error": "Xato matni", "errorCode": 40401 }`

`value` — endpoint qaytaradigan ma’lumot (masalan profil, kod, va hokazo).

---

## Autentifikatsiya (boshqa API’lar uchun)

OAuth 2.0 **password** yoki **client_credentials** orqali olingan token ishlatiladi:

```
Authorization: Bearer <access_token>
```

Token `/security/oauth/token` dan olinadi, muddati tugasa `/security/oauth/refresh_token` (yoki yangi token so‘rovida `grant_type=refresh_token`) orqali yangilanishi mumkin.

---

*Hujjat Rento API barcha endpointlari asosida tuzilgan. Yangilanish: 2025-03-02.*
