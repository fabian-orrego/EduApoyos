import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { API_ROUTES } from '../constants/api-routes';
import {
  CreateStudentRequest,
  CreateStudentResponse,
} from '../models/student.model';

/**
 * Thin HTTP wrapper for the <c>/api/estudiantes</c> endpoints (US-008). All error handling is
 * delegated to the global HTTP interceptors, so callers can focus on the happy path.
 */
@Injectable({ providedIn: 'root' })
export class StudentService {
  private readonly http = inject(HttpClient);

  create(request: CreateStudentRequest): Observable<CreateStudentResponse> {
    return this.http.post<CreateStudentResponse>(
      API_ROUTES.students.root,
      request,
    );
  }
}
