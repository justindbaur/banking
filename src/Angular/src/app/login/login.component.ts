import { Component } from '@angular/core';
import { FormBuilder, FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { lastValueFrom } from 'rxjs';
import { UserService } from '../services/user.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent {
  form = this.fb.nonNullable.group({
    username: new FormControl('', Validators.required),
  });

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private readonly userService: UserService,
  ) {}

  public async login() {
    if (!this.form.valid) {
      return;
    }

    await lastValueFrom(this.userService.login$(this.form.value.username!));

    await lastValueFrom(this.userService.refreshInfo$());

    await this.router.navigate(['/api-keys']);
  }
}
