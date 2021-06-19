import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = 'https://localhost:5001/api/account';
  constructor(private http: HttpClient) { }

  private getUrl(path: string): string {
    return `${this.baseUrl}/${path}`;
  }
  
  login(model: any) {
    return this.http.post(this.getUrl('login'), model);
  }
}
