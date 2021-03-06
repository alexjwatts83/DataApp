import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter<boolean>();

  registerForm!: FormGroup;
  maxDate: Date;
  validationErrors: string[];

  constructor(
      private accounterService: AccountService,
      private fb: FormBuilder,
      private router: Router) {
    this.maxDate = new Date();
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
    this.validationErrors = [];
  }

  ngOnInit(): void {
    this.initForm();
  }

  initForm() {
    this.registerForm = this.fb.group({
      gender: ['male'],
      username: ['', [Validators.required]],
      knownAs: ['', [Validators.required]],
      dateOfBirth: ['', [Validators.required]],
      city: ['', [Validators.required]],
      country: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: ['', [Validators.required,this.matchValuesValidator('password')]],
    });

    this.registerForm.controls.password.valueChanges.subscribe(() => {
      this.registerForm.controls.confirmPassword.updateValueAndValidity();
    });
  }

  matchValuesValidator(matchTo: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const parent = control.parent;
      const password = parent?.get(matchTo)?.value;
      const confirmPassword = control.value;
      const matched = password === confirmPassword;
      return matched ? null : { isMatched: { value: control.value } };
    };
  }

  register() {
    console.log({ form: this.registerForm });
    this.accounterService
      .register(this.registerForm.value)
      .subscribe((response: any) => {
        console.log('service register call sucessful', response);
        this.router.navigateByUrl('/members');
      },
      err => {
        console.error(err);
        this.validationErrors = err;
      });
  }

  cancel() {
    console.log('calling cancel from RegisterComponent');
    this.cancelRegister.emit(false);
  }
}
