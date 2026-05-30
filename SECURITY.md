# Security Notes

## Phase 0 Containment

The Phase 0 audit found real secret material in repository documentation and
deployment notes. Those values have been removed from the working tree, but any
secret that was committed or pasted into logs/transcripts must be treated as
compromised.

Rotate these credential categories before relying on production security:

- PostgreSQL production password and any connection strings containing it.
- JWT signing secret.
- Cloudinary cloud credentials.
- Neon or other development database credentials.
- SMTP/app-password credentials if they were ever shared outside a secret store.
- VPS SSH password or deploy key if it was exposed in chat, logs, screenshots, or commits.
- GitHub Actions deployment secrets after VPS credentials are rotated.

## History Cleanup

Redacting the current file contents is not enough if secrets were committed.
Recommended follow-up:

1. Rotate exposed secrets first.
2. Use a history-rewrite tool such as `git filter-repo` or BFG Repo-Cleaner to
   remove old secret values from git history.
3. Force-push only after coordinating with every collaborator.
4. Invalidate any forks, deployment caches, CI logs, terminal transcripts, or
   artifacts that may still contain old values.

## Secret Handling Rules

- Keep real values in GitHub Actions secrets, VPS `.env`, user secrets, or a
  dedicated secret manager.
- Keep `.env`, `*.env`, and `appsettings.Development.json` out of commits.
- Do not print full secret values to logs or support chats.
- Enable GitHub secret scanning and push protection.
- Add a pre-commit scanner such as `gitleaks` or `detect-secrets` before the next
  deployment push.

## Production Source Warning

During the audit, `https://api.appilico.com/swagger/v1/swagger.json` appeared to
advertise a different API surface from this repository. Do not deploy this repo
to production until the VPS checkout, GitHub remote, running container image, and
public API surface are verified to be the intended AppilicoShopServer source.

## Dependency Scanning

Before deploying, run:

```bash
dotnet list AppilicoShopServer.sln package --vulnerable --include-transitive
```

The Phase 2/3 modernization pass removed known vulnerable package versions. Keep
the scan clean before production deploys and document any exception in
[ARCHITECTURE.md](ARCHITECTURE.md).