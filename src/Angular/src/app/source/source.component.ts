import { Component } from '@angular/core';
import { SourceListItem, SourcesService } from './sources.service';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Observable, lastValueFrom, switchMap } from 'rxjs';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-source',
  standalone: true,
  template: `<ng-container *ngIf="source$ | async as source">
    <p>{{ source.displayName }}</p>
  </ng-container>`,
  imports: [RouterModule, CommonModule],
  providers: [SourcesService]
})
export class SourceComponent {
  source$: Observable<SourceListItem>;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly sourcesService: SourcesService
  ) {
    this.source$ = this.route.paramMap.pipe(
      switchMap((p) => this.sourcesService.get$(p.get('id')!))
    );
  }

  async delete(sourceId: string) {
    await lastValueFrom(this.sourcesService.delete$(sourceId))
  }
}
