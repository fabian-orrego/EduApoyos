import { Pipe, PipeTransform } from '@angular/core';

export type TipoApoyo = 'Tuition' | 'Transport' | 'Meals' | 'Materials' | string;

const LABELS: Record<string, string> = {
  Tuition: 'Tuition',
  Transport: 'Transport',
  Meals: 'Meals',
  Materials: 'Materials',
};

@Pipe({ name: 'tipoApoyo', standalone: true })
export class TipoApoyoPipe implements PipeTransform {
  transform(value: TipoApoyo | null | undefined): string {
    if (value == null) {
      return '';
    }
    return LABELS[value] ?? String(value);
  }
}
