import { environment } from '../../../environments/environment';

const base = environment.apiBaseUrl;

export const API_ROUTES = {
  auth: {
    login: `${base}/auth/login`,
    refresh: `${base}/auth/refresh`,
    logout: `${base}/auth/logout`,
  },
  students: {
    root: `${base}/students`,
    byId: (id: string | number) => `${base}/students/${id}`,
  },
  supportRequests: {
    root: `${base}/support-requests`,
    byId: (id: string | number) => `${base}/support-requests/${id}`,
  },
} as const;
