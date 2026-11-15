---
mode: ask
description: Review code for cross-tenant data leaks
---
Review the selected code. List every query, endpoint or page that could return, change or reveal data of a tenant
other than the caller's. For each, name the missing check (ForTenant, CanManage, route tenant, claim) and show the fix.
