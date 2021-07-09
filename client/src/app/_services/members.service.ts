import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../models/member';
import { PaginatedResult } from '../models/pagination';
import { UserParams } from '../models/usersParams';

@Injectable({
  providedIn: 'root',
})
export class MembersService {
  private baseUrl = `${environment.apiUrl}/users`;
  members: Member[] = [];
  memberCache = new Map();

  constructor(private http: HttpClient) {}

  private getUrl(path: string): string {
    return `${this.baseUrl}/${path}`;
  }

  private getPaginationHeaders(pageNumber: number, pageSize: number) {
    let params = new HttpParams();

    params = params.append('pageNumber', pageNumber.toString());
    params = params.append('pageSize', pageSize.toString());

    return params;
  }

  private convertToKey(userParams: UserParams) {
    return Object.values(userParams).join('-');
  }

  private getPaginatedResults<T>(url: string, params: HttpParams) {
    const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();
    return this.http.get<T>(url, { observe: 'response', params }).pipe(
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

  getMembers(userParams: UserParams) {
    var response = this.memberCache.get(this.convertToKey(userParams));
    if (response) {
      return of(response);
    }

    let params = this.getPaginationHeaders(
      userParams.pageNumber,
      userParams.pageSize
    );

    params = params.append('minAge', userParams.minAge.toString());
    params = params.append('maxAge', userParams.maxAge.toString());
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);

    return this.getPaginatedResults<Member[]>(this.baseUrl, params).pipe(
      map((response) => {
        this.memberCache.set(this.convertToKey(userParams), response);
        return response;
      })
    );
  }

  getMember(username: string) {
    const members = [...this.memberCache.values()].reduce(
      (arr, elem) => arr.concat(elem.result),
      []
    );

    // console.log({ members: members });
    const member = members.find((x: Member) => x.username === username);
    if (member != null) {
      return of(member);
    }
    
    return this.http.get<Member>(this.getUrl(username));
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl, member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = member;
      })
    );
  }

  setMainPhoto(photoId: number) {
    return this.http.put(this.getUrl(`set-main-photo/${photoId}`), {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.getUrl(`delete-photo/${photoId}`));
  }
}
