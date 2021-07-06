import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter<boolean>();
  model: any = {};
  registerForm!: FormGroup;

  constructor(private accounterService: AccountService, private toastr: ToastrService) { 
  }

  ngOnInit(): void {
    this.initForm()
  }

  initForm() {
    this.registerForm = new FormGroup({
      username: new FormControl(),
      password: new FormControl(),
      confirmPassword: new FormControl()
    });
  }

  register() {
    console.log({form: this.registerForm});
    
    // console.log('register', this.model);
    // this.accounterService
    //   .register(this.model)
    //   .subscribe((response: any) => {
    //     console.log('service register call sucessful', response);
    //     this.cancel();
    //   }, 
    //   err => {
    //     console.error(err)
    //     this.toastr.error(err.error);
    //   });
  }

  cancel() {
    console.log('calling cancel from RegisterComponent');
    this.cancelRegister.emit(false);
  }
}