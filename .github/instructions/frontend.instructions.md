---
name: VetGest frontend conventions
description: Apply to Blazor WebAssembly, Razor components, CSS, PWA, and frontend client files.
applyTo: "src/Frontend/**/*.cs,src/Frontend/**/*.razor,src/Frontend/**/*.css,src/Frontend/**/*.js,src/Frontend/**/*.json"
---

- Build mobile-first responsive interfaces with MudBlazor.
- Provide loading, empty, error, unauthorized, and offline states.
- Use typed API clients and keep presentation separate from state and transport logic.
- Never put secrets or private medical records in public service-worker caches.
- Use accessible labels, keyboard interaction, units, and clear emergency messaging.
- Load visualization code and 3D assets on demand with fallbacks for unsupported devices.
