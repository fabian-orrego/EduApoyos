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
