import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private baseUrl = `${environment.apiUrl}/account`;
  private currentUserSource = new ReplaySubject<User>(1);

  isLoggedIn$ = new BehaviorSubject<boolean>(false);
  currentUser$ = this.currentUserSource.asObservable();

  constructor(private http: HttpClient) { }

  private getUrl(path: string): string {
    return `${this.baseUrl}/${path}`;
  }
  
  login(model: any) {
    return this.http
    .post<User>(this.getUrl('login'), model)
    .pipe(
      map((user: User) => {
        if(user) {
          this.setCurrentUser(user);
        }
      })
    );
  }

  setCurrentUser(user: User): void {
    console.log(user);
    console.log(Object.entries(user));
    if(user.constructor  === Object) {
      console.log('user is undefined');
    }
    let isLogged = Object.entries(user).length > 0;
    console.log({isLogged: isLogged});
    this.isLoggedIn$.next(isLogged);
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUserSource.next(user);
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUserSource.next(undefined);
    this.isLoggedIn$.next(false);
  }

  register(model: any) {
    console.log('calling register with model', model);
    return this.http.post<User>(this.getUrl('register'), model)
    .pipe(
      map((user: User) => {
        console.log('api call successful', user);
        if(user) {
          this.setCurrentUser(user);
        }
        return user;
      })
    );
  }
}
