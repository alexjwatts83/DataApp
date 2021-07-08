import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../models/member';
import { PaginatedResult } from '../models/pagination';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private baseUrl = `${environment.apiUrl}/users`;
  members: Member[] = [];
  paginatedResult: PaginatedResult<Member[]> = new PaginatedResult<Member[]>();

  constructor(private http: HttpClient) { 
  }

  private getUrl(path: string): string {
    return `${this.baseUrl}/${path}`;
  }

  getMembers(page?: number, itemsPerPage?: number) {
    let params = new HttpParams();

    if (page != null && itemsPerPage != null) {
      params =  params.append('pageNumber', page.toString());
      params = params.append('pageSize', itemsPerPage.toString());
    }

    return this.http
      .get<Member[]>(this.baseUrl, { observe: 'response', params})
      .pipe(map(response=> {
        // console.log('response from member service', response);
        this.paginatedResult.result = response.body || undefined;
        // console.log(this.paginatedResult);
        if(response.headers.get('Pagination') != null) {
          this.paginatedResult.pagination = JSON.parse(response.headers.get('Pagination') || '{}') || undefined;
        }
        return this.paginatedResult;
      }));
  }

  getMember(username: string) {
    const member =  this.members.find(x => x.username === username);
    if(member != null) {
      return of(member);
    }
    return this.http.get<Member>(this.getUrl(username));
  }

  updateMember(member: Member) {
    return this.http
      .put(this.baseUrl, member)
      .pipe(map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = member;
      }));
  }

  setMainPhoto(photoId: number) {
    return this.http
      .put(this.getUrl(`set-main-photo/${photoId}`), {});
  }

  deletePhoto(photoId: number){
    return this.http.delete(this.getUrl(`delete-photo/${photoId}`))
  }
}
