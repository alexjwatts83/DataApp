import { HttpClient, HttpParams } from "@angular/common/http";
import { map } from "rxjs/operators";
import { PaginatedResult } from "../models/pagination";

export function getPaginationHeaders(pageNumber: number, pageSize: number) {
  let params = new HttpParams();

  params = params.append('pageNumber', pageNumber.toString());
  params = params.append('pageSize', pageSize.toString());

  return params;
}

export function getPaginatedResults<T>(url: string, params: HttpParams, http: HttpClient) {
  const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();
  return http.get<T>(url, { observe: 'response', params }).pipe(
    map((response) => {
      // console.log('response from member service', response);
      paginatedResult.result = response.body || undefined;
      // console.log(this.paginatedResult);
      if (response.headers.get('Pagination') != null) {
        paginatedResult.pagination =
          JSON.parse(response.headers.get('Pagination') || '{}') || undefined;
      }
      return paginatedResult;
    })
  );
}