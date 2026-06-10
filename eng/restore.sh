#!/usr/bin/env bash
source "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/common.sh"
CONFIG="$(write_local_nuget_config)"
NUGET_CERT_REVOCATION_MODE=offline "$DOTNET_CMD" restore "$ROOT/Pattrn.sln" --configfile "$CONFIG" /p:NuGetAudit=false -m:1 "$@"
