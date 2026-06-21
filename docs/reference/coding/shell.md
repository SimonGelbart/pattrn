# Shell Coding Standards

- Use Bash only when Bash features are required.
- Use `set -euo pipefail` for maintainer scripts.
- Quote variables unless word splitting is intentional.
- Detect missing tools and fail with useful instructions.
- Avoid hard-coded local paths in committed scripts when an environment variable or relative path can be used.
- Keep generated config files under ignored locations or deterministic maintainer-script paths.
