import { defineConfig } from 'astro/config';
import { remarkDocLinks } from './tools/remark-doc-links.mjs';

export default defineConfig({
  site: 'https://simongelbart.github.io',
  base: '/pattrn',
  trailingSlash: 'always',
  markdown: {
    remarkPlugins: [[remarkDocLinks, { manifestPath: 'docs.site.json', base: '/pattrn' }]],
  },
});
