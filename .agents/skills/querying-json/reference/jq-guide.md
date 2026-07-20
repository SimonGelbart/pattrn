# jq Quick Reference

`jq` is largely compatible with `jaq`. Use `jaq` only as a fallback when `jq` is unavailable or a filter behaves differently.

## Output Control

```bash
jq -r '.name' package.json       # raw string
jq -c '.items[]' data.json       # compact JSON
jq -e '.required' config.json    # fail for false or null
jq -S '.' data.json              # sort object keys
printf '%s\n' '{"name":"demo"}' | jq -r '.name'
```

## Common Filters

```bash
jq -r '.version' package.json
jq -r '.dependencies.react' package.json
jq '{name, version}' package.json
jq '.[0]' array.json
jq -r '.items[].name' data.json
jq -r '.dependencies | keys[]' package.json
jq '.items[] | select(.active)' data.json
jq '.items | length' data.json
jq '.items | map(.name) | sort' data.jsonq
jq -r '.field // "default"' data.json
jq -r '.items[]? // empty' data.json
jq -r 'has("requiredField")' data.json
```

## Variables and Updates

Pass shell values as data instead of interpolating them into filters:

```bash
jq --arg version "$VERSION" '.version = $version' package.json
jq --argjson enabled true '.features.enabled = $enabled' config.json
jq -s 'map(.items[]) | unique_by(.id)' shard-*.json
```

`jq` prints transformed JSON; it does not edit files in place. Write to a temporary file, validate it, then replace the original with the repository's preferred editing workflow.

## Shell Composition

Prefer compact or raw output before piping:

```bash
jq -r '.dependencies | keys[]' package.json | sort
jq -r '.dependencies | keys[]' package.json | while read -r dep; do
  rg -l -F "$dep"
done
```

## Edge Cases

- Use `// empty` to suppress absent optional values.
- Use `has("field")` when absence differs from null.
- Use `-e` in scripts that must fail on missing required data.
- Use `--arg` and `--argjson` for shell-provided values.
- Use `-s` to slurp multiple inputs into one array before combining them.
- Test complex filters if exact `jq` compatibility matters.
- Check the installed `jq` release before relying on optional non-JSON formats.
