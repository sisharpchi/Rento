# Rento — loyiha tahlili va “to'liq ishlashi” uchun qadamlar

## Loyiha tuzilishi

| Loyiha | Vazifasi |
|--------|----------|
| **Rento.AppHost.AppHost** | Aspire AppHost — PostgreSQL, Redis, ApiService, TelegramBot ni ishga tushiradi |
| **Rento.AppHost.ServiceDefaults** | Health check, service discovery, OpenTelemetry, HTTP resilience |
| **Rento.AppHost.ApiService** | ASP.NET Core Web API — OpenIddict auth, Telegram auth API |
| **Rento.TelegramBot** | Worker — Telegram bot (polling), ApiService ga HTTP orqali murojaat |
| **Rento.Application** | TelegramAuthService va boshqa application servislar |
| **Rento.Core** | Domain — User, UserRole, repository interfeyslari |
| **Rento.Contracts** | DTO va servis interfeyslari |
| **Rento.Infrastructure** | EF Core, PostgreSQL, Identity, OpenIddict, MainDbContext |
| **Rento.Shared** | ResponseResult, ErrorCodes |

---

## Nimalar tuzatildi / qo'shildi

1. **TelegramBot `Program.cs`**
   - `AddHttpClient` lambda yopilmagan edi (sintaksis xatosi) — tuzatildi.
   - Standalone ishlash uchun `RentoApi:BaseUrl` ishlatiladi; bo'sh bo'lsa `https+http://apiservice` (AppHost service discovery).

2. **AuthorizationController**
   - Qo'llab-quvvatlanmaydigan grant type uchun `NotImplementedException` o'rniga OpenIddict standartida `BadRequest` + `UnsupportedGrantType` qaytariladi.

3. **ApiService `Program.cs`**
   - `/weatherforecast` template endpoint olib tashlandi.
   - Startupda `MainDbContext` uchun `Migrate()` chaqiriladi — migratsiyalar mavjud bo'lsa DB avtomatik yangilanadi.

4. **EF Core migratsiya**
   - `Initial` migratsiyasi yaratilgan; ApiService ishga tushganda `db.Database.Migrate()` avtomatik pending migratsiyalarni qo'llaydi.
   - Kelajakda model o'zgarganda yangi migratsiya qo'shish:
   ```bash
   dotnet ef migrations add <MigrationName> --project src/Rento.Infrastructure/Rento.Infrastructure.csproj --startup-project src/Rento.AppHost/Rento.AppHost.ApiService/Rento.AppHost.ApiService.csproj
   ```
   - `dotnet-ef` o'rnatilmagan bo'lsa: `dotnet tool install --global dotnet-ef`

---

## To'liq ishlashi uchun siz qilishingiz kerak bo'lganlar

### 1. Maxfiy ma'lumotlar (majburiy)

- **Telegram bot**
  - [ ] `TelegramBot:BotToken` — BotFather dan olingan token (`Rento.TelegramBot/appsettings.json` yoki `appsettings.Development.json`, yoki User Secrets / muhit o'zgaruvchilari).
  - [ ] `TelegramBot:SecretKey` — bot va API o'rtasida bir xil bo'lishi kerak (ApiService va TelegramBot da bir xil qiymat).

- **ApiService**
  - [ ] `TelegramBot:SecretKey` — yuqoridagi bilan bir xil.
  - [ ] `ConnectionStrings:DefaultConnection` — PostgreSQL ulanish qatori (AppHost ostida Aspire avtomatik beradi; mustaqil ishlatishda `appsettings` yoki User Secrets da ko'rsating).

### 2. Bazaga migratsiya (majburiy)

- [x] Birinchi migratsiya (`Initial`) yaratilgan.
- [ ] ApiService ishga tushganda `Migrate()` avtomatik ishlaydi; ilova birinchi marta ishga tushganda PostgreSQL ishlab turgan bo'lishi va connection string to'g'ri bo'lishi kerak.

### 3. TelegramBot ni mustaqil ishlatish (ixtiyoriy)

- AppHost siz ishlatayotgan bo'lsangiz, `RentoApi:BaseUrl` bo'sh qoldirilsa, service discovery orqali `apiservice` ishlatiladi.
- Botni ApiService dan alohida (masalan, boshqa serverda) ishlatayotgan bo'lsangiz, TelegramBot `appsettings` da `RentoApi:BaseUrl` ni API manziliga qo'ying (masalan `https://localhost:5001` yoki production API URL).

### 4. Health check (ixtiyoriy)

- Hozir `/health` va `/alive` faqat **Development** da yoqilgan (ServiceDefaults).
- Production da ham health kerak bo'lsa, `Rento.AppHost.ServiceDefaults/Extensions.cs` da `MapDefaultEndpoints` ichida `if (app.Environment.IsDevelopment())` ni o'zgartirib, barcha muhitda map qilish mumkin (xavfsizlikni hisobga oling).

### 5. OAuth grant type (ixtiyoriy)

- Hozir: `client_credentials`, `password`, `refresh_token` qo'llab-quvvatlanadi.
- Authorization code flow kerak bo'lsa, OpenIddict sozlamalari va `AuthorizationController` ga qo'shimcha endpoint/logikani qo'shish kerak.

### 6. Bot tilini saqlash (ixtiyoriy)

- Callback da til tanlanganda “Til o'zgartirildi” chiqadi, lekin tanlangan til hali DB da saqlanmaydi va keyingi xabarlarda ishlatilmaydi. To'liq i18n uchun User/Profile da til maydoni va handler larda shu tilni ishlatish kerak.

---

## Ma'lumotlar oqimi (qisqacha)

1. **Telegram** → **Rento.TelegramBot** (polling: `/start`, callback).
2. **TelegramBot** → **ApiService** (HTTP, `X-Bot-Secret` header):
   - `POST api/auth/telegram/ensure-user`
   - `POST api/auth/telegram/code-for-bot`
   - `GET api/auth/telegram/profile?telegramUserId=...`
3. **ApiService** → **TelegramAuthController** → **ITelegramAuthService** → **MainDbContext** → **PostgreSQL**.
4. **OAuth**: `POST security/oauth/token` / `security/oauth/revoke` → **AuthorizationController** → OpenIddict + Identity → **PostgreSQL**.

---

## Tezkor tekshirish ro'yxati

- [ ] PostgreSQL ishlayapti (localhost yoki AppHost orqali).
- [ ] `TelegramBot:BotToken` va ikkala loyihada `TelegramBot:SecretKey` to'g'ri va bir xil.
- [ ] Birinchi migratsiya yaratilgan va ApiService ishga tushganda xato bermaydi.
- [ ] AppHost orqali ishlatilsa: AppHost → ApiService + TelegramBot birga ishga tushadi; bot `apiservice` ga service discovery orqali ulanadi.
- [ ] Mustaqil ishlatilsa: ApiService ni birinchi ishga tushiring, keyin TelegramBot ni `RentoApi:BaseUrl` bilan ishga tushiring.

Ushbu qadamlardan keyin loyiha to'liq ishlashi kerak. Qo'shimcha savol bo'lsa, aniq fayl yoki endpoint nomini yozing.
