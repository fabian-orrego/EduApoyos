import { environment } from '../../../environments/environment';

const base = environment.apiBaseUrl;

export const API_ROUTES = {
  auth: {
    login: `${base}/auth/login`,
    register: `${base}/auth/register`,
  },
  students: {
    root: `${base}/estudiantes`,
    byId: (id: string | number) => `${base}/estudiantes/${id}`,
  },
  supportRequests: {
    root: `${base}/solicitudes`,
    byId: (id: string | number) => `${base}/solicitudes/${id}`,
    status: (id: string | number) => `${base}/solicitudes/${id}/estado`,
    certificate: (id: string | number) => `${base}/solicitudes/${id}/constancia`,
  },
} as const;
