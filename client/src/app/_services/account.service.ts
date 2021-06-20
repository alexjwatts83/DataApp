import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private baseUrl = 'https://localhost:5001/api/account';
  private currentUserSource = new ReplaySubject<User>(1);
  currentUserSource$ = this.currentUserSource.asObservable();

  constructor(private http: HttpClient) { }

  private getUrl(path: string): string {
    return `${this.baseUrl}/${path}`;
  }
  
  login(model: any) {
    return this.http
    .post<User>(this.getUrl('login'), model)
    .pipe(
      map((response: User) => {
        const user = response;
        if(user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSource.next(user);
        }
      })
    );
  }

  setCurrentUser(user: User): void {
    this.currentUserSource.next(user);
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUserSource.next(undefined);
  }
}
