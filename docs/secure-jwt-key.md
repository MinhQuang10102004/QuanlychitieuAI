# Securing the JWT Signing Key

`Program.cs` now throws an exception unless `Jwt:Key` is provided via a secure source. The placeholder value in `appsettings*.json` is intentionally invalid to prevent accidental commits of real secrets.

## Recommended Approaches

### 1. .NET User Secrets (local development)

```bash
cd c:\Users\DELL\source\repos\QuanLyChiTieu\QuanLyChiTieu
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "<a-strong-random-string>"
```

- The secret is stored outside the repository under `%APPDATA%\Microsoft\UserSecrets`.
- `dotnet run`/`dotnet watch` automatically reads the key from user secrets.

### 2. Environment Variables (CI/CD or servers)

Set an environment variable before starting the app:

```cmd
setx JWT__KEY "<a-strong-random-string>"
```

or for the current session only:

```cmd
set JWT__KEY=<a-strong-random-string>
dotnet run
```

### 3. Azure App Settings / Docker Secrets

- **Azure App Service:** configure `Jwt:Key` in the portal under *Configuration > Application Settings*.
- **Docker:** mount the key as a secret/file and map it into an environment variable during container startup.

## Generating a Strong Key

Use at least 32 random bytes. Examples:

```bash
# PowerShell
[Convert]::ToBase64String((New-Object Security.Cryptography.RNGCryptoServiceProvider).GetBytes(32))

# OpenSSL
openssl rand -base64 32
```

## Verifying Configuration

After setting the key, run:

```cmd
dotnet run
```

If the key is missing or still the placeholder, the app will throw:

```
InvalidOperationException: Jwt:Key must be provided via secrets or environment variables.
```

## Why This Matters

- A leaked signing key allows attackers to forge valid tokens.
- Rotating the key periodically invalidates old tokens—remember to inform users to re-login after rotation.
