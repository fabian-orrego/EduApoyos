import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { API_ROUTES } from '../constants/api-routes';
import { PagedResult } from '../models/paged-result.model';
import {
  ChangeSupportRequestStatusRequest,
  ChangeSupportRequestStatusResponse,
  CreateSupportRequestRequest,
  CreateSupportRequestResponse,
  SupportRequestDetail,
  SupportRequestListFilters,
  SupportRequestListItem,
  UpdateSupportRequestRequest,
  UpdateSupportRequestResponse,
} from '../models/support-request.model';

export interface SupportRequestCertificateDownload {
  blob: Blob;
  fileName: string;
}

/**
 * Thin HTTP wrapper for the <c>/api/solicitudes</c> endpoints (US-013 → US-018). Error
 * handling is delegated to the global HTTP interceptors so callers can focus on the happy
 * path.
 */
@Injectable({ providedIn: 'root' })
export class SupportRequestService {
  private readonly http = inject(HttpClient);

  create(
    request: CreateSupportRequestRequest,
  ): Observable<CreateSupportRequestResponse> {
    return this.http.post<CreateSupportRequestResponse>(
      API_ROUTES.supportRequests.root,
      request,
    );
  }

  list(
    pageNumber: number,
    pageSize: number,
    filters: SupportRequestListFilters,
  ): Observable<PagedResult<SupportRequestListItem>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    if (filters.status !== null) {
      params = params.set('status', filters.status);
    }
    if (filters.supportType !== null) {
      params = params.set('supportType', filters.supportType);
    }
    if (filters.fromDate) {
      params = params.set('fromDate', filters.fromDate);
    }
    if (filters.toDate) {
      params = params.set('toDate', filters.toDate);
    }

    return this.http.get<PagedResult<SupportRequestListItem>>(
      API_ROUTES.supportRequests.root,
      { params },
    );
  }

  getById(id: string): Observable<SupportRequestDetail> {
    return this.http.get<SupportRequestDetail>(
      API_ROUTES.supportRequests.byId(id),
    );
  }

  update(
    id: string,
    request: UpdateSupportRequestRequest,
  ): Observable<UpdateSupportRequestResponse> {
    return this.http.put<UpdateSupportRequestResponse>(
      API_ROUTES.supportRequests.byId(id),
      request,
    );
  }

  changeStatus(
    id: string,
    request: ChangeSupportRequestStatusRequest,
  ): Observable<ChangeSupportRequestStatusResponse> {
    return this.http.patch<ChangeSupportRequestStatusResponse>(
      API_ROUTES.supportRequests.status(id),
      request,
    );
  }

  downloadCertificate(id: string): Observable<SupportRequestCertificateDownload> {
    const params = new HttpParams().set('_', Date.now().toString());
    const headers = new HttpHeaders({
      'Cache-Control': 'no-cache',
      Pragma: 'no-cache',
    });

    return this.http
      .get(API_ROUTES.supportRequests.certificate(id), {
        responseType: 'blob',
        observe: 'response',
        params,
        headers,
      })
      .pipe(
        map((response) => {
          const blob = response.body ?? new Blob();
          const fileName =
            this.readFileName(response.headers.get('Content-Disposition')) ??
            this.buildFallbackFileName(id);
          return { blob, fileName };
        }),
      );
  }

  private readFileName(contentDisposition: string | null): string | null {
    if (!contentDisposition) {
      return null;
    }

    const utf8Match = /filename\*=UTF-8''([^;]+)/i.exec(contentDisposition);
    if (utf8Match?.[1]) {
      return decodeURIComponent(utf8Match[1].trim());
    }

    const plainMatch = /filename="?([^";]+)"?/i.exec(contentDisposition);
    return plainMatch?.[1]?.trim() ?? null;
  }

  private buildFallbackFileName(id: string): string {
    const stamp = Math.random().toString(16).slice(2, 10);
    return `constancia-${id.slice(0, 8)}-${stamp}.pdf`;
  }
}
