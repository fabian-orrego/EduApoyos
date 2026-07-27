import { DocumentTypeIdValue } from './student.model';

export const SupportTypeId = {
  Scholarship: 1,
  Loan: 2,
  Subsidy: 3,
} as const;

export type SupportTypeIdValue =
  (typeof SupportTypeId)[keyof typeof SupportTypeId];

export interface SupportTypeOption {
  id: SupportTypeIdValue;
  label: string;
}

export const SUPPORT_TYPE_OPTIONS: readonly SupportTypeOption[] = [
  { id: SupportTypeId.Scholarship, label: 'Beca' },
  { id: SupportTypeId.Loan, label: 'Préstamo' },
  { id: SupportTypeId.Subsidy, label: 'Subsidio' },
] as const;

export const SUPPORT_TYPE_LABELS: Readonly<Record<SupportTypeIdValue, string>> =
  SUPPORT_TYPE_OPTIONS.reduce(
    (acc, option) => {
      acc[option.id] = option.label;
      return acc;
    },
    {} as Record<SupportTypeIdValue, string>,
  );

export const SupportRequestStatusId = {
  Pending: 1,
  UnderReview: 2,
  Approved: 3,
  Rejected: 4,
} as const;

export type SupportRequestStatusIdValue =
  (typeof SupportRequestStatusId)[keyof typeof SupportRequestStatusId];

export interface SupportRequestStatusOption {
  id: SupportRequestStatusIdValue;
  label: string;
}

export const SUPPORT_REQUEST_STATUS_OPTIONS: readonly SupportRequestStatusOption[] =
  [
    { id: SupportRequestStatusId.Pending, label: 'Pendiente' },
    { id: SupportRequestStatusId.UnderReview, label: 'En Revisión' },
    { id: SupportRequestStatusId.Approved, label: 'Aprobada' },
    { id: SupportRequestStatusId.Rejected, label: 'Rechazada' },
  ] as const;

export const SUPPORT_REQUEST_STATUS_LABELS: Readonly<
  Record<SupportRequestStatusIdValue, string>
> = SUPPORT_REQUEST_STATUS_OPTIONS.reduce(
  (acc, option) => {
    acc[option.id] = option.label;
    return acc;
  },
  {} as Record<SupportRequestStatusIdValue, string>,
);

/** Allowed status transitions per US-016 (encoded on the client for UX gating). */
export const SUPPORT_REQUEST_STATUS_TRANSITIONS: Readonly<
  Record<SupportRequestStatusIdValue, readonly SupportRequestStatusIdValue[]>
> = {
  [SupportRequestStatusId.Pending]: [SupportRequestStatusId.UnderReview],
  [SupportRequestStatusId.UnderReview]: [
    SupportRequestStatusId.Approved,
    SupportRequestStatusId.Rejected,
  ],
  [SupportRequestStatusId.Approved]: [],
  [SupportRequestStatusId.Rejected]: [],
};

/** True when the request is in a terminal state and cannot be edited (US-016 RN-2/RN-3). */
export function isFinalizedStatus(
  status: SupportRequestStatusIdValue,
): boolean {
  return (
    status === SupportRequestStatusId.Approved ||
    status === SupportRequestStatusId.Rejected
  );
}

/** Payload for <c>POST /api/solicitudes</c> (US-013). */
export interface CreateSupportRequestRequest {
  studentEmail: string;
  supportType: SupportTypeIdValue;
  requestedAmount: number;
  description: string;
}

export interface CreateSupportRequestResponse {
  id: string;
  studentId: string;
  supportType: SupportTypeIdValue;
  requestedAmount: number;
  description: string;
  status: SupportRequestStatusIdValue;
  requestedAt: string;
}

/** Payload for <c>PUT /api/solicitudes/{id}</c> (US-016 edit). */
export interface UpdateSupportRequestRequest {
  supportType: SupportTypeIdValue;
  requestedAmount: number;
  description: string;
}

export interface UpdateSupportRequestResponse {
  id: string;
  studentId: string;
  supportType: SupportTypeIdValue;
  requestedAmount: number;
  description: string;
  status: SupportRequestStatusIdValue;
  requestedAt: string;
  updatedAt: string;
}

/** Payload for <c>PATCH /api/solicitudes/{id}/estado</c> (US-016 status). */
export interface ChangeSupportRequestStatusRequest {
  newStatus: SupportRequestStatusIdValue;
  notes: string | null;
}

export interface ChangeSupportRequestStatusResponse {
  id: string;
  previousStatus: SupportRequestStatusIdValue;
  newStatus: SupportRequestStatusIdValue;
  advisorId: string;
  updatedAt: string;
}

/** Row projection returned by <c>GET /api/solicitudes</c> (US-015). */
export interface SupportRequestListItem {
  id: string;
  studentFullName: string;
  studentDocumentNumber: string;
  supportType: SupportTypeIdValue;
  status: SupportRequestStatusIdValue;
  requestedAmount: number;
  requestedAt: string;
}

/** Full projection returned by <c>GET /api/solicitudes/{id}</c> (US-014). */
export interface SupportRequestDetail {
  id: string;
  studentId: string;
  studentFullName: string;
  studentEmail: string;
  studentDocumentNumber: string;
  studentDocumentType: DocumentTypeIdValue;
  studentAcademicProgram: string;
  studentSemester: number;
  supportType: SupportTypeIdValue;
  requestedAmount: number;
  description: string;
  status: SupportRequestStatusIdValue;
  requestedAt: string;
  updatedAt: string;
  advisorId: string | null;
  advisorFullName: string | null;
  history: readonly SupportRequestHistoryItem[];
}

export interface SupportRequestHistoryItem {
  id: string;
  previousStatus: SupportRequestStatusIdValue;
  newStatus: SupportRequestStatusIdValue;
  changedAt: string;
  changedByUserId: string;
  changedByFullName: string;
  notes: string | null;
}

/** Optional filters applied to the advisor grid (US-015). */
export interface SupportRequestListFilters {
  status: SupportRequestStatusIdValue | null;
  supportType: SupportTypeIdValue | null;
  fromDate: string | null;
  toDate: string | null;
}
