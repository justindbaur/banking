import { Component } from '@angular/core';
import { SourceListItem, SourcesService } from './sources.service';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Observable, lastValueFrom, map, shareReplay, switchMap } from 'rxjs';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-source',
  standalone: true,
  template: `<ng-container *ngIf="source$ | async as source">
    <p>{{ source.displayName ?? source.sourceId }}</p>
  </ng-container>`,
  imports: [RouterModule, CommonModule],
  providers: [SourcesService],
})
export class SourceComponent {
  source$: Observable<SourceListItem>;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly sourcesService: SourcesService
  ) {
    const sourceId$ = this.route.paramMap.pipe(
      map((p) => p.get('id')!),
      shareReplay(1)
    );

    this.source$ = sourceId$.pipe(
      switchMap((id) => this.sourcesService.get$(id))
    );
  }

  async delete(sourceId: string) {
    await lastValueFrom(this.sourcesService.delete$(sourceId));
  }
}
