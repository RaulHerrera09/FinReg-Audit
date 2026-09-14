# FinReg.DemoProvisioner

One-shot operator tool that creates or rotates the public demo Auditor account. It is not part of the API image and exposes no endpoint.

## What it changes

- Only `auditor@demo.finreg.dev`, with the fixed role `Auditor`. Email and role cannot be passed as arguments.
- Missing account: created with a BCrypt hash (work factor 12, via `BCryptPasswordHasher`).
- Existing Auditor account with a different password: the hash is replaced and that account's active refresh tokens are revoked.
- Existing account with the same password: nothing is written.
- Existing account with another role, or pending EF Core migrations: the tool stops without changes.
- All writes run in a single database transaction. It never applies migrations or seeds other data.

`DatabaseSeeder` only inserts demo users that are missing, so it does not overwrite a rotated password.

## Inputs

| Input | Source |
|---|---|
| `DATABASE_URL` | Environment variable (Npgsql keyword format). If absent and a terminal is attached, it is requested as hidden input. |
| Password | Hidden prompt, entered twice, when a terminal is attached. Otherwise one line from stdin. A leading UTF-8 byte order mark is discarded. Minimum 12 characters. |

The tool never prints the connection string, the password, the hash or any token.

## Exit codes

| Code | Meaning |
|---:|---|
| 0 | Created, rotated or unchanged |
| 1 | Database or connection error, nothing committed |
| 2 | Missing `DATABASE_URL` or invalid password input |
| 3 | Pending migrations, nothing changed |
| 4 | Email exists with another role, nothing changed |

## Run locally

```powershell
dotnet run --project tools/FinReg.DemoProvisioner
```

The tool asks for `DATABASE_URL` and the password without echoing them.

## Run inside the Fly.io machine

This keeps `DATABASE_URL` inside the machine that already has it. Use Windows `tar.exe`; Git's GNU tar treats `C:` as a remote host.

```powershell
$Work = Join-Path $env:TEMP 'finreg-provisioner'
dotnet publish tools/FinReg.DemoProvisioner -c Release -o "$Work\bin" --no-self-contained
& "$env:SystemRoot\System32\tar.exe" --format ustar -czf "$Work\provisioner.tgz" -C "$Work\bin" .

flyctl ssh sftp put --app finreg-audit --machine <machine-id> -q "$Work\provisioner.tgz" /tmp/finreg-provisioner.tgz

# Single quotes keep $? and $rc for the remote shell. Temporary files are removed even when the tool fails.
$Remote = 'sh -c ''mkdir -p /tmp/finreg-provisioner && tar -xzf /tmp/finreg-provisioner.tgz -C /tmp/finreg-provisioner && dotnet /tmp/finreg-provisioner/FinReg.DemoProvisioner.dll; rc=$?; rm -rf /tmp/finreg-provisioner /tmp/finreg-provisioner.tgz; echo provisioner-exit=$rc'''
Read-Host 'Demo password' | flyctl ssh console --app finreg-audit --machine <machine-id> -C $Remote
```

Rely on the printed `provisioner-exit=<code>` rather than the `flyctl` exit code. Afterwards, confirm that `POST /api/auth/login` returns HTTP 200 for the demo account.
