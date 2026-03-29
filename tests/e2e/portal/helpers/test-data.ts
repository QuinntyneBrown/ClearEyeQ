export const TEST_CLINICIAN_EMAIL =
  process.env.TEST_CLINICIAN_EMAIL || 'thompson@cleareyeq.com';

export const TEST_CLINICIAN_PASSWORD =
  process.env.TEST_CLINICIAN_PASSWORD || 'TestPass123!';

export const TEST_TENANT_ID =
  process.env.TEST_TENANT_ID || 'tenant-cleareyeq-dev';

export const API_BASE_URL =
  process.env.API_BASE_URL || 'http://localhost:3000';

/** Known patient IDs from mock data */
export const TEST_PATIENT_ID = 'p1';
export const TEST_PATIENT_NAME = 'Sarah Chen';
export const TEST_PATIENT_STATUS = 'Flagged';

export const TEST_PATIENT_STABLE_ID = 'p2';
export const TEST_PATIENT_STABLE_NAME = 'Michael Park';

export const TEST_PATIENT_URGENT_ID = 'p4';
export const TEST_PATIENT_URGENT_NAME = 'David Kim';

/** Known referral IDs from mock data */
export const TEST_REFERRAL_URGENT_PATIENT = 'Sarah Chen';
export const TEST_REFERRAL_STANDARD_PATIENT = 'David Kim';

/** Known treatment review IDs from mock data */
export const TEST_REVIEW_PATIENT = 'Sarah Chen';
export const TEST_REVIEW_DIAGNOSIS = 'Allergic Conjunctivitis';

/** Nav section labels as they appear in the sidebar */
export const NAV_SECTIONS = [
  'Dashboard',
  'Patients',
  'Treatment Reviews',
  'Referrals',
  'Settings',
] as const;

/** Routes for each nav section */
export const NAV_ROUTES: Record<string, string> = {
  Dashboard: '/',
  Patients: '/patients',
  'Treatment Reviews': '/treatment-reviews',
  Referrals: '/referrals',
  Settings: '/settings',
};
