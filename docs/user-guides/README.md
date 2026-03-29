# ClearEyeQ User Guides

This folder contains end-user and operator guidance for the three primary ClearEyeQ applications defined in the current product requirements:

- Patient Mobile App
- Clinical Portal
- Admin Portal

> [!NOTE]
> These guides are based on the current requirements and architecture documents in this repository. They describe the intended product workflow and operating model, which may evolve as the applications move from design and scaffolding into full implementation.

## Guides

- [Patient Mobile App User Guide](patient-mobile-app.md)
- [Clinical Portal User Guide](clinical-portal.md)
- [Admin Portal User Guide](admin-portal.md)

## Workflow Diagrams

- [Patient Mobile App Workflow](patient-mobile-app-workflow.png)
- [Clinical Portal Workflow](clinical-portal-workflow.png)
- [Admin Portal Workflow](admin-portal-workflow.png)

## Source References

- [L1 requirements](../specs/L1.md)
- [L2 requirements](../specs/L2.md)
- [System architecture overview](../detailed-design/00-system-architecture/overview.md)
- [Identity and access design](../detailed-design/01-identity-and-access/overview.md)
- [Clinical portal design](../detailed-design/08-clinical-portal/overview.md)
- [Subscription and billing design](../detailed-design/10-subscription-and-billing/overview.md)

## Regenerating Diagram Images

The PNG images in this folder are generated from the PlantUML files alongside them.

```bash
python docs/detailed-design/render-puml.py docs/user-guides
```
