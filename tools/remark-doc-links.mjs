import fs from 'node:fs';
import path from 'node:path';

const externalProtocolPattern = /^(?:[a-z][a-z0-9+.-]*:|\/\/)/i;

export function remarkDocLinks(options = {}) {
  const manifestPath = options.manifestPath ?? 'docs.site.json';
  const base = normalizeBase(options.base ?? '/pattrn');
  const manifest = JSON.parse(fs.readFileSync(path.resolve(process.cwd(), manifestPath), 'utf8'));
  const routeBySource = new Map(
    manifest.docs.map((doc) => [normalizeSource(doc.source), normalizeRoute(doc.route)])
  );
  const sourceRoot = (manifest.project?.sourceRoot ?? 'https://github.com/SimonGelbart/pattrn/blob/main').replace(/\/$/, '');

  return (tree, file) => {
    const currentSource = normalizeSource(path.relative(process.cwd(), file.path ?? ''));
    visit(tree, (node) => {
      if (node.type !== 'link' || typeof node.url !== 'string') return;

      const rewritten = rewriteUrl({
        url: node.url,
        currentSource,
        routeBySource,
        sourceRoot,
        base,
      });

      if (rewritten) node.url = rewritten;
    });
  };
}

function rewriteUrl({ url, currentSource, routeBySource, sourceRoot, base }) {
  if (!url || url.startsWith('#') || externalProtocolPattern.test(url)) return null;

  const { pathname, suffix } = splitUrl(url);
  if (!pathname.endsWith('.md')) return null;

  const targetSource = normalizeSource(path.posix.normalize(path.posix.join(path.posix.dirname(currentSource), decodeURIComponent(pathname))));
  const route = routeBySource.get(targetSource);

  if (route) return `${base}${route}${suffix}`;

  if (targetSource.startsWith('docs/') || targetSource === 'README.md' || targetSource === 'AGENTS.md') {
    return `${sourceRoot}/${targetSource}${suffix}`;
  }

  return null;
}

function splitUrl(url) {
  const match = url.match(/^([^?#]*)([?#].*)?$/);
  return {
    pathname: match?.[1] ?? url,
    suffix: match?.[2] ?? '',
  };
}

function normalizeSource(source) {
  return source.replace(/\\/g, '/').replace(/^\.\//, '').replace(/\/+/g, '/');
}

function normalizeRoute(route) {
  const prefixed = route.startsWith('/') ? route : `/${route}`;
  return prefixed.endsWith('/') ? prefixed : `${prefixed}/`;
}

function normalizeBase(base) {
  if (!base || base === '/') return '';
  return base.startsWith('/') ? base.replace(/\/$/, '') : `/${base.replace(/\/$/, '')}`;
}

function visit(node, callback) {
  if (!node || typeof node !== 'object') return;

  callback(node);

  for (const value of Object.values(node)) {
    if (Array.isArray(value)) {
      for (const child of value) visit(child, callback);
    } else if (value && typeof value === 'object') {
      visit(value, callback);
    }
  }
}
