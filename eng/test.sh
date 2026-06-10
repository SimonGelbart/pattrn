#!/usr/bin/env bash
source "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/common.sh"
NUGET_CERT_REVOCATION_MODE=offline "$DOTNET_CMD" test "$ROOT/Pattrn.sln" --configuration Release --no-build "$@"
