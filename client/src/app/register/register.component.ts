import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
  @Input() usersFromHomeComponent: any;
  model: any = {};
  constructor() { 
  }

  ngOnInit(): void {
    console.log('regiser:usersFromHomeComponent', this.usersFromHomeComponent)
  }

  register() {
    console.log('register', this.model);
  }

  cancel() {
    console.log('cancelled');
  }
}
