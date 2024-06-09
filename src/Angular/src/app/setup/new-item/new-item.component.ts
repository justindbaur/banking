import { AsyncPipe, CommonModule, KeyValuePipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, Inject, Input } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
} from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Observable, map, switchMap } from 'rxjs';
import { BASE_URL } from '../../app.module';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

type FormInput = {
  type: 'text' | 'textarea' | 'number';
  label: string;
  hideInput: boolean;
  description?: string;
  order?: number;
};

type FormSelect = {
  type: 'select';
  label: string;
  description?: string;
  values: Record<string, string>;
  order?: number;
};

type AnyFormInput = FormInput | FormSelect;

export type StartToken = {
  requirementsSchema: { inputs: Record<string, AnyFormInput> };
  state: string | null;
};

@Component({
  standalone: true,
  imports: [
    AsyncPipe,
    KeyValuePipe,
    CommonModule,
    MatButtonModule,
    ReactiveFormsModule,
    MatInputModule,
  ],
  templateUrl: './new-item.component.html',
})
export class NewItemComponent {
  start$: Observable<{
    state: string | null;
    inputs: Record<string, AnyFormInput>;
    formGroup: FormGroup;
  }>;

  constructor(
    route: ActivatedRoute,
    private readonly httpClient: HttpClient,
    @Inject(BASE_URL) private readonly baseUrl: string,
    private readonly formBuilder: FormBuilder
  ) {
    const sourceId = route.paramMap.pipe(map((p) => p.get('id')!));

    this.start$ = sourceId.pipe(
      switchMap((id) => {
        return this.httpClient.get<StartToken>(
          `${this.baseUrl}/sources/${id}/start`
        );
      }),
      map((startToken) => {
        console.log(startToken.requirementsSchema.inputs);
        const controls: Record<string, FormControl> = {};
        const inputs = Object.entries(
          startToken.requirementsSchema.inputs
        ).sort(
          ([aKey, aValue], [bKey, bValue]) =>
            (aValue.order ?? 0) - (bValue.order ?? 0)
        );

        console.log(inputs);
        for (const key of Object.keys(Object.fromEntries(inputs))) {
          controls[key] = this.formBuilder.control('', { nonNullable: true });
        }

        return {
          state: startToken.state,
          inputs: Object.fromEntries(inputs),
          formGroup: this.formBuilder.group(controls),
        };
      })
    );
  }

  submit(form: FormGroup, state: string | null) {
    console.log('form', form);
  }
}
