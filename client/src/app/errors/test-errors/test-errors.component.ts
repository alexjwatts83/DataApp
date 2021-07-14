import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-test-errors',
  templateUrl: './test-errors.component.html',
  styleUrls: ['./test-errors.component.scss']
})
export class TestErrorsComponent implements OnInit {
  private baseUrl = `${environment.apiUrl}`;
  validationErrors: string[] = [];

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
  }

  private getUrl(path: string) {
    return `${this.baseUrl}/buggy/${path}`
  }

  get404Error() {
    this.http.get(this.getUrl(`not-found`)).subscribe(response => {
      console.log(response);
    }, error=> {
      console.log(error);
    })
  }

  get400Error() {
    this.http.get(this.getUrl(`bad-request`)).subscribe(response => {
      console.log(response);
    }, error=> {
      console.log(error);
    })
  }

  get400NoTextError() {
    this.http.get(this.getUrl(`bad-request-no-text`)).subscribe(response => {
      console.log(response);
    }, error=> {
      console.log(error);
    })
  }

  get500Error() {
    this.http.get(this.getUrl(`server-error`)).subscribe(response => {
      console.log(response);
    }, error=> {
      console.log(error);
    })
  }

  get401Error() {
    this.http.get(this.getUrl(`auth`)).subscribe(response => {
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
    const url = `${this.baseUrl}/account/register`;
    this.http.post(url,model).subscribe(response => {
      console.log(response);
    }, error=> {
      console.log(error);
      this.validationErrors = error;
    })
  }
}
