# Rento Mini App — API hujjati

Faqat **Telegram Mini App** ishlatadigan API. Boshqa endpointlar (Bot, Register, OAuth va boshqalar) bu hujjatda yo'q.

**Base URL:** `https://<host>/` (masalan: `https://localhost:5xxx/` yoki production API manzili)

**Content-Type:**  
- `/api/auth/telegram/*` — `application/json`  
- `/security/oauth/token` — `application/x-www-form-urlencoded` (form body)

**Autentifikatsiya:** Kod so‘rash va token olish uchun header/token kerak emas. Token olgach, boshqa API’lar uchun `Authorization: Bearer <access_token>` ishlatiladi.

---

## Login kodi yaratish (generate-code)

**POST** `/api/auth/telegram/generate-code`

Foydalanuvchi allaqachon ro'yxatdan o'tgan yoki botda `/start` bosgan bo'lishi kerak. Telefon raqam orqali login kodi so'raladi. Kod **2 daqiqa** amal qiladi.

### Request

```json
{
  "phoneNumber": "+998901234567"
}
```

| Maydon      | Tur   | Majburiy | Tavsif   |
|-------------|-------|----------|----------|
| phoneNumber | string| Ha       | Telefon  |

### Muvaffaqiyatli javob — 200 OK

```json
{
  "success": true
}
```

### Xato — 404 Not Found (foydalanuvchi topilmadi)

Foydalanuvchi bu telefon bilan ro'yxatdan o'tmagan yoki Telegram bilan ulanmagan.

```json
{
  "success": false,
  "error": "User not found",
  "errorCode": 40401
}
```

### Xato — 400 Bad Request (boshqa)

```json
{
  "success": false,
  "error": "Xato xabari",
  "errorCode": 40002
}
```

| errorCode | Ma'nosi     |
|-----------|-------------|
| 40401     | UserNotFound |
| 40002     | CodeExpired (oldingi kod hali amalda, yangi so‘rash vaqtida cheklov bo‘lishi mumkin) |

---

## Token olish (password grant) — login kod bilan

Kod yaratilgandan keyin foydalanuvchi shu kodni "parol" sifatida yuborib access token (va ixtiyoriy refresh token) oladi.

**POST** `/security/oauth/token`  
**Content-Type:** `application/x-www-form-urlencoded` (form body, JSON emas).

### Request (form body)

| Maydon       | Qiymat           | Tavsif |
|--------------|------------------|--------|
| grant_type   | `password`       | Majburiy |
| username     | `+998901234567`  | Telefon raqam (yoki email) — generate-code da ishlatilgan raqam |
| password     | `1234`           | Login kodi (generate-code dan keyin foydalanuvchiga yuborilgan 4 xonali kod) |
| scope        | `offline_access` | Ixtiyoriy — berilsa javobda `refresh_token` ham chiqadi |

**Misol (form-urlencoded):**
```
grant_type=password&username=%2B998901234567&password=1234&scope=offline_access
```

### Muvaffaqiyatli javob — 200 OK

```json
{
  "token_type": "Bearer",
  "access_token": "CfDJ8...",
  "expires_in": 86400,
  "refresh_token": "CfDJ8..."
}
```

| Maydon        | Tavsif |
|---------------|--------|
| access_token  | Keyingi API so'rovlarida `Authorization: Bearer <access_token>` sifatida ishlatiladi |
| expires_in    | Sekundlarda (odatda 86400 = 24 soat) |
| refresh_token | Faqat `scope=offline_access` bersangiz; token yangilash uchun |

### Xato — 401 Unauthorized

```json
{
  "error": "invalid_grant",
  "error_description": "User not found."
}
```
yoki
```json
{
  "error": "invalid_grant",
  "error_description": "Invalid password."
}
```

- **User not found** — bu telefon/username bo‘yicha foydalanuvchi yo‘q.
- **Invalid password** — kod noto‘g‘ri yoki muddati tugagan (2 daqiqadan oshgan).

### Xato — 400 Bad Request

```json
{
  "error": "invalid_request",
  "error_description": "Username and password are required."
}
```

`username` yoki `password` bo‘sh yuborilganda.

---

## Xato kodlari (Mini App uchun)

| Kod   | Nomi          | Tavsif                          |
|-------|---------------|----------------------------------|
| 40000 | InvalidRequest| Noto'g'ri so'rov / maydonlar     |
| 40001 | InvalidPhone  | Noto'g'ri telefon formati        |
| 40002 | CodeExpired   | Kod muddati tugagan              |
| 40401 | UserNotFound  | Foydalanuvchi topilmadi (telefon yoki ulanish yo'q) |

---

## Mini App tipik oqim

1. **Start** — Mini App ochilganda Telegram `initData` dan `user.id` (telegramUserId) olingan bo‘ladi.
2. **Kod so‘rash** — Foydalanuvchi telefonini kiritadi → `POST /api/auth/telegram/generate-code` (phoneNumber) → kod bot orqali yuboriladi.
3. **Token olish** — Foydalanuvchi kodni kiritadi → `POST /security/oauth/token` (grant_type=password, username=telefon, password=kod, ixtiyoriy scope=offline_access) → `access_token` (va kerak bo‘lsa `refresh_token`) olinadi.
4. **Keyingi so‘rovlar** — Barcha himoyalangan API’lar uchun header: `Authorization: Bearer <access_token>`.

---

*Faqat Mini App uchun. To‘liq API: [README.md](./README.md).*
