# Copilot usage notes

This project used GitHub Copilot to accelerate review and remediation work for the MEMS assignment update.

## How Copilot was used
- Reviewed the assignment specification from the PDF and translated key requirements into implementation tasks.
- Identified the existing MVC baseline and mapped each missing requirement to the most relevant code path.
- Helped check validation, security, and model consistency while keeping the work limited to the web project.
- Suggested the targeted changes needed for tenant-aware filtering, slug support, and session-only ballot storage.

## Example prompts
- "Review the assignment requirements and map the missing multi-tenant changes to the existing ASP.NET Core project."
- "Update the municipality model to include tenant slug support and enforce unique public URLs."
- "Switch preferred candidate persistence from local storage to browser session storage without exposing ballot data to the backend."

## Productivity gains
- Faster identification of the existing code paths and required touch points.
- Quicker validation of schema/model changes before finalizing the patch.
- Reduced manual review effort when reconciling the old MVC project with the assignment's modern multi-tenant requirements.

## Issues encountered
- The repo still reflected the earlier Assignment #1 MVC design, so some requirements were not implemented in the first pass.
- The public preference list originally used browser localStorage, which conflicted with the privacy requirement for session-only ballot data.
- Required database changes needed a migration to keep the model and runtime schema aligned.
