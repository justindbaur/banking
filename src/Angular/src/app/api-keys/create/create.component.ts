import { Component } from '@angular/core';
import { FormBuilder, FormControl, Validators } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { lastValueFrom } from 'rxjs';

@Component({
  selector: 'app-create-api-key',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss'],
})
export class CreateComponent {
  form = this.fb.nonNullable.group({
    name: new FormControl('', Validators.required),
    expiration: new FormControl('', Validators.required),
  });

  constructor(
    private fb: FormBuilder,
    private readonly apiService: ApiService
  ) {}

  async create() {
    console.log(this.form.value);
    const date = new Date();
    const expiration = this.form.value.expiration;
    let expirationDate: Date | undefined = undefined;
    if (expiration === 'custom') {
      // TODO: implement custom expiration
    } else if (expiration === 'never') {
      expirationDate = undefined;
    } else if (expiration != undefined) {
      const minutes = parseInt(expiration);
      if (Number.isNaN(minutes)) {
        throw new Error(`Invalid expiration: ${expiration}`);
      }

      expirationDate = new Date(date.getTime() + minutes * 60 * 1000);
    }

    await lastValueFrom(this.apiService.createApiKey$({}));
  }
}
