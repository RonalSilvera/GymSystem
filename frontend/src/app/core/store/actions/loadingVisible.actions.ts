import { createAction, props } from '@ngrx/store';

export const setLoadingVisible = createAction(
  '[LOAD PANEL] SET',
  props<{ loadingVisible: boolean }>()
)
