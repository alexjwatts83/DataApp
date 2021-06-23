import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
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

  constructor(private accounterService: AccountService, private toastr: ToastrService) { 
  }

  ngOnInit(): void {
  }

  register() {
    console.log('register', this.model);
    this.accounterService
      .register(this.model)
      .subscribe((response: any) => {
        console.log('service register call sucessful', response);
        this.cancel();
      }, 
      err => {
        console.error(err)
        this.toastr.error(err.error);
      });
  }

  cancel() {
    console.log('calling cancel from RegisterComponent');
    this.cancelRegister.emit(false);
  }
}