import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormControl,
  FormGroup,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter<boolean>();
  model: any = {};
  registerForm!: FormGroup;
  // public form: FormGroup;
  constructor(
    private accounterService: AccountService,
    private toastr: ToastrService,
    private fb: FormBuilder
  ) {
    this.registerForm = new FormGroup({
      username: new FormControl('', [Validators.required]),
      password: new FormControl('', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]),
      confirmPassword: new FormControl('', [Validators.required, this.matchValuesValidator('password') ])
    },
    // this.passwordsShouldMatch
  );
  }

  ngOnInit(): void {
    // this.initForm();
  }

  private checkPassword(control: FormControl) {
    return control.value.toString().length >= 5 && control.value.toString().length <= 10
      ? null
      : {'outOfRange': true};
  }

  private passwordsShouldMatch(fGroup: FormGroup) {
    return fGroup?.get('password')?.value === fGroup?.get('passwordConfirm')?.value
      ? null : {'mismatch': true};
  }

  matchValuesValidator(matchTo: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const parent = control.parent;
      const password = parent?.get(matchTo)?.value;
      const confirmPassword = control.value;
      const matched = password === confirmPassword;
      // console.clear();
      // console.log({password:password,confirmPassword:confirmPassword, matched: matched });

      return matched ? null : {isMatched: {value: control.value}};
    };
  }

  register() {
    console.log({ form: this.registerForm });

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
