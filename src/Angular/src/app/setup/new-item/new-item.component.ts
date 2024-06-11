import {
  AsyncPipe,
  CommonModule,
  KeyValue,
  KeyValuePipe,
} from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, Inject, Input } from '@angular/core';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Observable, lastValueFrom, map, switchMap, tap } from 'rxjs';
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

export type StepResponse = {
  answers: unknown;
  state: string | null;
};

type CompleteResumeToken = { isComplete: true, sourceId: string };
type IncompleteResumeToken = {
  isComplete: false;
  requirementsSchema: { inputs: Record<string, AnyFormInput> };
  state: string;
};

export type ResumeToken = CompleteResumeToken | IncompleteResumeToken;

@Component({
  standalone: true,
  imports: [
    AsyncPipe,
    KeyValuePipe,
    CommonModule,
    MatButtonModule,
    ReactiveFormsModule,
    MatInputModule,
    RouterModule,
  ],
  templateUrl: './new-item.component.html',
})
export class NewItemComponent {
  start$: Observable<{
    sourceId: string;
    state: string | null;
    inputs: Record<string, AnyFormInput>;
    formGroup: FormGroup;
  }>;

  sortOrder(
    a: KeyValue<string, AnyFormInput>,
    b: KeyValue<string, AnyFormInput>
  ) {
    return (a.value.order ?? 0) - (b.value.order ?? 0);
  }

  constructor(
    route: ActivatedRoute,
    private readonly httpClient: HttpClient,
    @Inject(BASE_URL) private readonly baseUrl: string,
    private readonly formBuilder: FormBuilder,
    private readonly router: Router,
  ) {
    const sourceId = route.paramMap.pipe(map((p) => p.get('id')!));

    this.start$ = sourceId.pipe(
      switchMap((id) => {
        return this.httpClient
          .get<StartToken>(`${this.baseUrl}/sources/${id}/start`, {
            withCredentials: true,
          })
          .pipe(
            map((startToken) => {
              const controls: Record<string, FormControl> = {};
              const inputs = Object.fromEntries(
                Object.entries(startToken.requirementsSchema.inputs)
              );

              for (const key of Object.keys(inputs)) {
                controls[key] = this.formBuilder.control('', {
                  nonNullable: true,
                });
              }

              return {
                sourceId: id,
                state: startToken.state,
                inputs: inputs,
                formGroup: this.formBuilder.group(controls),
              };
            })
          );
      })
    );
  }

  async submit(sourceId: string, form: FormGroup, state: string | null) {
    this.start$ = this.httpClient
      .post<ResumeToken>(
        `${this.baseUrl}/sources/${sourceId}/resume`,
        {
          answers: form.getRawValue(),
          state: state,
        },
        { withCredentials: true }
      )
      .pipe(
        map((resumeToken) => {
          if (resumeToken.isComplete) {
            void this.router.navigate(["sources", resumeToken.sourceId]);
            throw new Error('Unreachable');
          }

          const controls: Record<string, FormControl> = {};
          const inputs = Object.fromEntries(
            Object.entries(resumeToken.requirementsSchema.inputs)
          );

          for (const key of Object.keys(inputs)) {
            controls[key] = this.formBuilder.control('', {
              nonNullable: true,
            });
          }

          return {
            sourceId: sourceId,
            state: resumeToken.state,
            inputs: inputs,
            formGroup: this.formBuilder.group(controls),
          };
        })
      );
  }
}
