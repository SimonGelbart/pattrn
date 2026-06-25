import manifest from '../../../../docs.site.json';

export type SiteDoc = {
  source: string;
  route: string;
  title: string;
  section: string;
  render?: boolean;
  status?: string;
};

export type SiteLink = {
  label: string;
  href: string;
  external: boolean;
};

export type DocsManifest = {
  project: {
    name: string;
    slug: string;
    repo: string;
    sourceRoot: string;
  };
  docs: SiteDoc[];
  exclude?: string[];
};

export const docsManifest = manifest as DocsManifest;

const base = import.meta.env.BASE_URL.replace(/\/$/, '');

export function withBase(route: string) {
  const normalizedRoute = route.startsWith('/') ? route : `/${route}`;
  return `${base}${normalizedRoute}`;
}

export function getAllDocs() {
  return docsManifest.docs;
}

export function getRenderableDocs() {
  return docsManifest.docs.filter((doc) => doc.render !== false);
}

export function getDocByRoute(route: string) {
  const normalizedRoute = normalizeRoute(route);
  return docsManifest.docs.find((doc) => normalizeRoute(doc.route) === normalizedRoute);
}

export function getRouteForSource(source: string) {
  const normalizedSource = normalizeSource(source);
  return docsManifest.docs.find((doc) => normalizeSource(doc.source) === normalizedSource)?.route;
}

export function getSourceUrl(source: string) {
  return `${docsManifest.project.sourceRoot}/${normalizeSource(source)}`;
}

export function isExternalHref(href: string) {
  return /^(?:https?:)?\/\//.test(href);
}

export function routeLink(route: string, label: string): SiteLink {
  const href = withBase(route);
  return { label, href, external: isExternalHref(href) };
}

export function sourceLink(source: string, label = source): SiteLink {
  const href = getSourceUrl(source);
  return { label, href, external: true };
}

export function linkForSource(source: string, label = source): SiteLink {
  const route = getRouteForSource(source);
  return route ? routeLink(route, label) : sourceLink(source, label);
}

export function routeToParam(route: string) {
  return normalizeRoute(route).replace(/^\//, '').replace(/\/$/, '');
}

export function getTopLevelKey(section: string) {
  switch (section) {
    case 'Reference':
    case 'Maintainer guidance':
      return 'reference';
    case 'Packages':
      return 'packages';
    case 'ADRs':
      return 'adr';
    case 'Roadmap':
      return 'roadmap';
    case 'Benchmarks':
      return 'benchmarks';
    default:
      return 'docs';
  }
}

export function getDocsForSection(section: string, { renderableOnly = false } = {}) {
  const docs = renderableOnly ? getRenderableDocs() : getAllDocs();
  return docs.filter((doc) => doc.section === section);
}

export function getRenderableDocsBySection(section: string) {
  return getDocsForSection(section, { renderableOnly: true });
}

export function getDocsBySection(): { section: string; docs: SiteDoc[] }[];
export function getDocsBySection(section: string): SiteDoc[];
export function getDocsBySection(section?: string) {
  if (section) {
    return getDocsForSection(section, { renderableOnly: true });
  }

  const sections = new Map<string, SiteDoc[]>();

  for (const doc of getRenderableDocs()) {
    const sectionDocs = sections.get(doc.section) ?? [];
    sectionDocs.push(doc);
    sections.set(doc.section, sectionDocs);
  }

  return Array.from(sections, ([section, docs]) => ({ section, docs }));
}

export function normalizeSource(source: string) {
  return source.replace(/\\/g, '/').replace(/^\.\//, '').replace(/\/+/g, '/');
}

export function normalizeRoute(route: string) {
  const prefixed = route.startsWith('/') ? route : `/${route}`;
  return prefixed.endsWith('/') ? prefixed : `${prefixed}/`;
}
