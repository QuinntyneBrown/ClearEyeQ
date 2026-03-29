export const ADMIN_CREDENTIALS = {
  email: process.env.TEST_ADMIN_EMAIL || 'admin@cleareyeq.com',
  password: process.env.TEST_ADMIN_PASSWORD || 'Admin123!@#',
};

export const NON_ADMIN_CREDENTIALS = {
  email: 'user@cleareyeq.com',
  password: 'User123!@#',
};

export const TEST_TENANT = {
  id: '00000000-0000-0000-0000-000000000001',
  name: 'Test Clinic Alpha',
};

export const TEST_TENANT_CREATE_NAME = `E2E Test Tenant ${Date.now()}`;

export const ROUTES = {
  dashboard: '/',
  tenants: '/tenants',
  tenantDetail: (id: string) => `/tenants/${id}`,
  users: '/users',
  subscriptions: '/subscriptions',
  systemHealth: '/system/health',
  featureFlags: '/system/features',
  auditLog: '/audit',
} as const;

export const NAV_SECTIONS = [
  { label: 'Dashboard', href: '/' },
  { label: 'Tenants', href: '/tenants' },
  { label: 'Users', href: '/users' },
  { label: 'Subscriptions', href: '/subscriptions' },
  { label: 'System Health', href: '/system/health' },
  { label: 'Feature Flags', href: '/system/features' },
  { label: 'Audit Log', href: '/audit' },
] as const;

export const SERVICE_NAMES = [
  'API Gateway',
  'Patient Intake',
  'Clinical Workflow',
  'Diagnostic Engine',
  'Imaging Pipeline',
  'Billing & Claims',
  'Notifications',
  'Identity & Access',
  'Analytics',
  'Telehealth',
  'Scheduling',
  'Reporting',
] as const;

export const SUBSCRIPTION_TIERS = ['Free', 'Pro', 'Premium', 'Autonomous'] as const;

export const USER_ROLES = ['Patient', 'Clinician', 'Admin'] as const;

export const AUDIT_ACTIONS = [
  'Create',
  'Update',
  'Delete',
  'Login',
  'Logout',
  'RoleChange',
  'StatusChange',
] as const;
