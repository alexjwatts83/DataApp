import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-test-errors',
  templateUrl: './test-errors.component.html',
  styleUrls: ['./test-errors.component.scss']
})
export class TestErrorsComponent implements OnInit {
  private baseUrl = 'https://localhost:5001/api/buggy';
  validationErrors: string[] = [];

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
  }

  get404Error() {
    this.http.get(`${this.baseUrl}/not-found`).subscribe(response => {
      console.log(response);
    }, error=> {
      console.log(error);
    })
  }

  get400Error() {
    this.http.get(`${this.baseUrl}/bad-request`).subscribe(response => {
      console.log(response);
    }, error=> {
      console.log(error);
    })
  }

  get400NoTextError() {
    this.http.get(`${this.baseUrl}/bad-request-no-text`).subscribe(response => {
      console.log(response);
    }, error=> {
      console.log(error);
    })
  }

  get500Error() {
    this.http.get(`${this.baseUrl}/server-error`).subscribe(response => {
      console.log(response);
    }, error=> {
      console.log(error);
    })
  }

  get401Error() {
    this.http.get(`${this.baseUrl}/auth`).subscribe(response => {
      console.log(response);
    }, error=> {
      console.log(error);
    })
  }

  get400ValidationError() {
    var model = {
      username: '',
      password: ''
    };
    this.http.post(`https://localhost:5001/api/account/register`,model).subscribe(response => {
      console.log(response);
    }, error=> {
      console.log(error);
      this.validationErrors = error;
    })
  }
}
