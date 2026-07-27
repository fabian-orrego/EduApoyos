import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { API_ROUTES } from '../constants/api-routes';
import { PagedResult } from '../models/paged-result.model';
import {
  CreateStudentRequest,
  CreateStudentResponse,
  StudentListItem,
  UpdateStudentRequest,
  UpdateStudentResponse,
} from '../models/student.model';

/**
 * Thin HTTP wrapper for the <c>/api/estudiantes</c> endpoints (US-008 to US-011). All error
 * handling is delegated to the global HTTP interceptors, so callers can focus on the happy path.
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

  list(
    pageNumber: number,
    pageSize: number,
  ): Observable<PagedResult<StudentListItem>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    return this.http.get<PagedResult<StudentListItem>>(
      API_ROUTES.students.root,
      { params },
    );
  }

  update(
    id: string,
    request: UpdateStudentRequest,
  ): Observable<UpdateStudentResponse> {
    return this.http.put<UpdateStudentResponse>(
      API_ROUTES.students.byId(id),
      request,
    );
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(API_ROUTES.students.byId(id));
  }
}
