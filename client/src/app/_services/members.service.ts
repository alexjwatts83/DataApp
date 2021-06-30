import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Member } from '../models/member';

const httpOptions = {
  headers: new HttpHeaders({
    Authorization: `Bearer ${(JSON.parse(localStorage.getItem('user') || `{}`).token)}`
  })
}

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private baseUrl = `${environment.apiUrl}/users`;
  constructor(private http: HttpClient) { 
    console.log(httpOptions);
  }

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
