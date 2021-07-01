import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Member } from '../models/member';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private baseUrl = `${environment.apiUrl}/users`;
  constructor(private http: HttpClient) { 
  }

  private getUrl(path: string): string {
    return `${this.baseUrl}/${path}`;
  }

  getMembers() {
    return this.http.get<Member[]>(this.baseUrl);
  }

  getMember(username: string) {
    return this.http.get<Member[]>(this.getUrl(username));
  }
}
