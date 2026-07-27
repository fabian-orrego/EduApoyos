export const DocumentTypeId = {
  NationalId: 1,
  ForeignerId: 2,
  Passport: 3,
} as const;

export type DocumentTypeIdValue =
  (typeof DocumentTypeId)[keyof typeof DocumentTypeId];

export interface DocumentTypeOption {
  id: DocumentTypeIdValue;
  label: string;
}

export const DOCUMENT_TYPE_OPTIONS: readonly DocumentTypeOption[] = [
  { id: DocumentTypeId.NationalId, label: 'Cédula de ciudadanía' },
  { id: DocumentTypeId.ForeignerId, label: 'Cédula de extranjería' },
  { id: DocumentTypeId.Passport, label: 'Pasaporte' },
] as const;

/**
 * Lookup for the user-facing document type label given its integer identifier.
 * Used by the students grid (US-011) to render the enum value coming from the API.
 */
export const DOCUMENT_TYPE_LABELS: Readonly<Record<DocumentTypeIdValue, string>> =
  DOCUMENT_TYPE_OPTIONS.reduce(
    (acc, option) => {
      acc[option.id] = option.label;
      return acc;
    },
    {} as Record<DocumentTypeIdValue, string>,
  );

/**
 * Fixed catalog of academic programs offered to Advisors when registering a student
 * (US-008 – field validation #4). Kept as constants so the select never allows free text.
 */
export const ACADEMIC_PROGRAM_OPTIONS: readonly string[] = [
  'Ingeniería de Software',
  'Ingeniería Industrial',
  'Administración de Empresas',
  'Contaduría Pública',
] as const;

/** Valid semesters allowed by RN-005 (1..12 inclusive). */
export const SEMESTER_OPTIONS: readonly number[] = Array.from(
  { length: 12 },
  (_, index) => index + 1,
);

export interface CreateStudentRequest {
  email: string;
  documentNumber: string;
  documentType: DocumentTypeIdValue;
  academicProgram: string;
  semester: number;
}

export interface CreateStudentResponse {
  id: string;
  userId: string;
  documentNumber: string;
  documentType: DocumentTypeIdValue;
  academicProgram: string;
  semester: number;
}

/** Payload for <c>PUT /api/estudiantes/{id}</c> (US-009). */
export interface UpdateStudentRequest {
  documentNumber: string;
  documentType: DocumentTypeIdValue;
  academicProgram: string;
  semester: number;
}

/** Response returned by <c>PUT /api/estudiantes/{id}</c> (US-009). */
export interface UpdateStudentResponse {
  id: string;
  userId: string;
  documentNumber: string;
  documentType: DocumentTypeIdValue;
  academicProgram: string;
  semester: number;
}

/**
 * Row shape returned by <c>GET /api/estudiantes</c> (US-011). The advisor grid needs the
 * student's identity information plus the linked user's full name and email.
 */
export interface StudentListItem {
  id: string;
  fullName: string;
  documentNumber: string;
  documentType: DocumentTypeIdValue;
  academicProgram: string;
  semester: number;
  email: string;
}
