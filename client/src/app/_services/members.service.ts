import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../models/member';

const httpOptions = {
  headers: new HttpHeaders({
    Authorization: `Bearer ${JSON.parse(JSON.parse(localStorage.getItem('user') || `{token: '' }`).token)}`
  })
}

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private baseUrl = `${environment.apiUrl}/users`;
  constructor(private http: HttpClient) { }

  private getUrl(path: string): string {
    return `${this.baseUrl}/${path}`;
  }

  getMembers() {
    return this.http.get<Member[]>(this.baseUrl, httpOptions);
  }

  getMember(username: string) {
    return this.http.get<Member[]>(this.getUrl(username), httpOptions);
  }
}
