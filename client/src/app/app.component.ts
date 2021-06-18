import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'Almighty Push';
  users: any;
  restApiUrl: string;
  constructor(private httpClient: HttpClient) {
    this.restApiUrl = 'https://localhost:44304/api/users';
  }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers() {
    this.httpClient.get(this.restApiUrl).subscribe((data : any) => {
      this.users = data;
    });
    console.log({users: this.users});
  }
}
