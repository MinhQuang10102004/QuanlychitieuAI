# Frontend Authentication Guide

This project now issues JWT access tokens when users sign in via `POST api/nguoidung/dangnhap`. A web client must:

1. **Authenticate** by posting email/password and keep the returned token.
2. **Attach** the token to every subsequent API request as a bearer token.
3. **Handle expiry** by redirecting users to the sign-in screen when the token is rejected.

## Sample Login Request (TypeScript/JavaScript)

```ts
async function login(email: string, password: string) {
  const response = await fetch("https://localhost:5001/api/nguoidung/dangnhap", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ email, matKhau: password }),
  });

  if (!response.ok) {
    throw new Error("Đăng nhập thất bại");
  }

  const data = await response.json();
  localStorage.setItem("access_token", data.token);
  localStorage.setItem("token_expires_at", data.expiresAt);

  return data.user; // { maNguoiDung, hoTen, email }
}
```

## Attaching the Token to API Calls

```ts
function getAuthHeaders() {
  const token = localStorage.getItem("access_token");
  if (!token) {
    throw new Error("Token không tồn tại, vui lòng đăng nhập lại");
  }

  return {
    Authorization: `Bearer ${token}`,
  };
}

async function fetchTransactions() {
  const response = await fetch("https://localhost:5001/api/giaodich", {
    headers: {
      "Content-Type": "application/json",
      ...getAuthHeaders(),
    },
  });

  if (response.status === 401) {
    // Token hết hạn hoặc không hợp lệ
    window.location.href = "/login";
    return;
  }

  return response.json();
}
```

## Token Expiration Handling

- `DangNhapResponseDto` returns an `expiresAt` (UTC). Compare it with the current time before calling APIs; if expired, prompt the user to log in again.
- Optional: implement a background timer to remove the token from storage shortly before expiry.

## CORS

`Program.cs` enables a `Frontend` CORS policy for `http://localhost:5173` and `http://localhost:3000`. Adjust or extend this list to match the actual frontend origins.

1. Update `Program.cs` if your frontend runs on a different port/domain.
2. Restart the API after modifying origins so the policy is reloaded.

## Recommended Next Steps

- Store tokens in memory or secure cookies if you want to mitigate XSS (localStorage is acceptable for internal tools but less secure).
- Add logout logic that clears stored tokens.
- Consider implementing refresh tokens or silent re-authentication for better UX.
